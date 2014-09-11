using System;
using System.Collections;
using System.Diagnostics;

namespace OOD.Imp.Storage
{
	/// <summary>
	/// The b-tree will be used to manage the stored class information.
	/// </summary>
	public class CatalogTree
	{
		private			BTree			m_tree;
		private			uint			m_next_cid;
		private			Hashtable		m_name_cache;
		private			Hashtable		m_cid_cache;
		private			int				m_cache_limit = 300;
		private			ClassEntry		m_LRU; //maintain the least recently used list for cached class info

		public CatalogTree(uint topNodeSId, SegmentManager sgManager, uint nextCId)
		{
			m_tree = new BTree(topNodeSId, sgManager, new KCatalog());
			m_next_cid = nextCId;
			m_cid_cache = new Hashtable(m_cache_limit);
			m_name_cache = new Hashtable(m_cache_limit);
			m_LRU = new ClassEntry();
		}	

		public uint TopNodeSId
		{
			get { return m_tree.TopNodSegId;}
		}

		public uint NextCId
		{
			get { return m_next_cid;}
		}

		public bool Insert(string className, string[] fields, uint topNodeSId, ref uint assignedCId)
		{
			if (m_name_cache.ContainsKey(className))
				return false;

			//no duplicates allowed.
			if (Search(className) != null)
				return false;

			assignedCId = m_next_cid++;
			KCatalog key = new KCatalog(className);
			DCatalog data = new DCatalog(assignedCId, fields, topNodeSId);
			key.ClassInfo = data;

			bool succeed = m_tree.Insert(key);
			if (succeed)
				_put_in_cache(assignedCId, className, fields, topNodeSId);
			return succeed;
		}

		public ClassEntry Search(string className)
		{
			if (m_name_cache.ContainsKey(className))
			{
				ClassEntry result = (ClassEntry)m_name_cache[className];
				result.BreakLinks();
				result.BuildLinks(m_LRU, m_LRU.Next);
				return result;
			}
			else
			{
				//not in cache, get it from disk
				KCatalog target = new KCatalog(className);
				KCatalog t = (KCatalog)m_tree.Search(target);
				if (t != null)
				{
					_put_in_cache(t.ClassInfo.CId, className, t.ClassInfo.FieldNames, t.ClassInfo.TopNodeSId);
					return (ClassEntry)m_cid_cache[t.ClassInfo.CId];
				}
				else
					return null;
			}
		}

		public ClassEntry Search(uint cid)
		{
			//time consuming operations, that's the reason for cache
			if (m_cid_cache.ContainsKey(cid))
			{
				ClassEntry t = (ClassEntry)m_cid_cache[cid];
				t.BreakLinks();
				t.BuildLinks(m_LRU, m_LRU.Next);
				return t;
			}
			else
			{
				//do the sequential search, :(
				OOD.Imp.Storage.BTree.BTEnumerator bt = m_tree.GetEnumerator();
				while (bt.MoveNext())
				{
					KCatalog key = (KCatalog)bt.Current;
					Debug.Assert(key != null);
					if (key.ClassInfo != null && key.ClassInfo.CId == cid)
					{
						//found
						_put_in_cache(cid, key.ClassName, key.ClassInfo.FieldNames, key.ClassInfo.TopNodeSId);
						return (ClassEntry)m_cid_cache[cid];						
					}
				}
				return null;
			}
		}

		public bool UpdateTopNodeSId(string className, uint newTopNodeSId)
		{
			ClassEntry oldValue = Search(className);
			if (oldValue != null)
			{
				//update cache
				((ClassEntry)m_cid_cache[oldValue.CId]).SetTopNodeSId(newTopNodeSId);
				//update the b-tree file
				KCatalog newKey = new KCatalog(className);
				newKey.ClassInfo = new DCatalog(oldValue.CId, oldValue.FieldNames, newTopNodeSId);
				if (m_tree.UpdateData(newKey) == false)
					throw new OOD.Exception.ProgramLogicError(
						this,
						"Update operation failed for class info b-tree.");
				return true;
			}
			else
				return false;
		}

		public bool Delete(string className)
		{
			//remove it from cache if needed
			if (m_name_cache.ContainsKey(className))
			{
				ClassEntry t = (ClassEntry)m_name_cache[className];
				t.BreakLinks();
				m_name_cache.Remove(className);
				m_cid_cache.Remove(t.CId);
			}
			return m_tree.Delete(new KCatalog(className));
		}

		internal void _put_in_cache(uint cid, string className, string[] fields, uint topNodeSId)
		{
			Debug.Assert(m_name_cache.Count == m_cid_cache.Count);

			if (m_name_cache.Count > m_cache_limit)
			{
				//kick one out needed
				ClassEntry victim = m_LRU.Previous;

				Debug.Assert(victim != null);

				victim.BreakLinks(); //remove from LRU list
				m_name_cache.Remove(victim.ClassName);
				m_cid_cache.Remove(victim.CId);
			}
			
			ClassEntry ce = new ClassEntry(cid, className, fields, topNodeSId);
			m_name_cache.Add(className, ce);
			m_cid_cache.Add(cid, ce);
			ce.BuildLinks(m_LRU, m_LRU.Next); //put it to the front of the list, -- most recently used
		}
	}
}
