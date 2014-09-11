using System;
using System.Diagnostics;

namespace OOD
{
	/// <summary>
	/// A generic hashtable with LRU kick out policy enforced.
	/// </summary>
	public class LRUHashtable
	{

		private			int						m_size;
		private			LRUHashEntry[]			m_items;
		private			int						m_count;
		private			int						m_cache_limit;
		private			LRUHashEntry			m_LRU;



		public LRUHashtable(int size, int limit)
		{
			m_size = size;
			m_items = new LRUHashEntry[size];
			m_count = 0;
			m_cache_limit = limit;
			m_LRU = new LRUHashEntry();
		}

		public int Count
		{
			get { return m_count;}
		}

		public bool Insert(object key, object data)
		{
			if (m_count >= m_cache_limit)
			{
				//kick one out
				LRUHashEntry victim = m_LRU.LRU_Prev;
				bool t = Delete(victim.Key);
				Debug.Assert(t);
			}

			int pos = (int)(Math.Abs(key.GetHashCode()) % m_size);

			LRUHashEntry head = m_items[pos];

			if (head == null)
			{
				LRUHashEntry ent = new LRUHashEntry(key, data);
				m_items[pos] = ent;
				//put his into the front of the LRU list
				ent.BuildLinks(m_LRU, m_LRU.LRU_Next);
			}
			else
			{
				while (head.Next != null &&head.Key != key)
				{
					head = head.Next;
				}
				if (head.Key.Equals(key))
					throw new OOD.Exception.ProgramLogicError(this, "Insert an existing key into the hashtable");
				else
				{
					LRUHashEntry e = new  LRUHashEntry(key, data);
                    head.Next = e;
					e.BuildLinks(m_LRU, m_LRU.LRU_Next);
				}
			}

			m_count ++;
			return true;
		}

		public bool Exists(object key)
		{
			int pos = (int)(Math.Abs(key.GetHashCode()) % m_size);

			LRUHashEntry head = m_items[pos];

			if (head == null)
				return false;
			else
			{
				while (head != null)
				{
					if (head.Key.Equals(key))
						return true;
					head = head.Next;
				}
				return false;
			}
		}

		public bool Delete(object key)
		{
			int pos = (int)(Math.Abs(key.GetHashCode()) % m_size);

			LRUHashEntry head = m_items[pos];

			if (head == null)
				return false;
			else
			{
				LRUHashEntry prev = null;
				while (head != null)
				{
					if (head.Key.Equals(key))
						break;
					prev = head;
					head = head.Next;
				}
				if (head != null) //found
				{
					if (prev == null)
						m_items[pos] = head.Next;
					else
						prev.Next = head.Next;

					//remove from LRU
					head.BreakLinks();

					m_count--;
					return true;
				}
				else
					return false;
			}
		}

		public object Retrive(object key)
		{
			int pos = (int)(Math.Abs(key.GetHashCode()) % m_size);

			LRUHashEntry head = m_items[pos];

			if (head == null)
				return null;
			else
			{
				while (head != null)
				{
					if (head.Key.Equals(key))
						break;
					head = head.Next;
				}
				if (head != null) //found
				{
					//move head to the front of the m_LRU
					head.BuildLinks(m_LRU, m_LRU.LRU_Next);
					return head.Data;
				}
				else
					return null;
			}
		}


		public MyEnumerator GetEnumerator() 
		{
			return new MyEnumerator(this);
		}

		// Declare the enumerator class:
		public class MyEnumerator 
		{
			LRUHashEntry m_current = null;
			LRUHashtable m_table;
			int m_row = 0;
			public MyEnumerator(LRUHashtable table) 
			{
				m_table = table;
			}

			public bool MoveNext() 
			{
				if (m_current == null)
				{
					while (m_row < m_table.m_size && m_table.m_items[m_row] == null)
					{
						m_row ++;
					}
					if (m_row < m_table.m_size)
						m_current = m_table.m_items[m_row];
					else
						m_current = null;
					m_row ++;
				}
				else
				{
					if (m_current.Next != null)
						m_current = m_current.Next;
					else
					{
						while (m_row < m_table.m_size && m_table.m_items[m_row] == null)
						{
							m_row ++;
						}
						if (m_row < m_table.m_size)
							m_current = m_table.m_items[m_row];
						else
							m_current = null;
						m_row ++;
					}
				}

				return m_current != null;
			}

			public object Current 
			{
				get 
				{
					return m_current.Data;
				}
			}

		}
	}
}
