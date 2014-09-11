using System;
using System.Reflection;
using System.Diagnostics;
using OOD.Imp.Storage;
using System.Collections;
using System.Text.RegularExpressions;

namespace OOD
{
	/// <summary>
	/// Take care of the internal storage, such as initialze page management and
	/// segment management.
	/// </summary>
	public class ObjectStore
	{

		private			OOD.Imp.Storage.DiskFile				m_dbFile;
		private			OOD.Imp.Storage.DiskFile				m_indexFile;
		private			OOD.Imp.Storage.memSegmentManager		m_indexPageManager;
		private			OOD.Imp.Storage.SegTree					m_segmentIndexTree;
		private			OOD.Imp.Storage.SpaceTree				m_spaceIndexTree;
		private			OOD.Imp.Storage.dbSegmentManager		m_dbSegmentManager;
		private			OOD.Imp.Storage.CatalogTree				m_catalogTree;

		private			short									m_segTreeNodeOrder = 26; //given the node size = 512, each key 14 bytes
		private			short									m_spaceTreeNodeOrder = 36; //given the key size 8 bytes
		private			short									m_catalogTreeNodeOrder = 21;
		private			short									m_classTreeNodeOrder = 21;

		private			LRUHashtable							m_classCache;
		private			int										m_classCacheLimit = 100;
		private			LRUHashtable							m_objectTreeCache;
		private			int										m_objectTreeCacheLimit = 100;
		private			int										m_cache_size = 59;


		/// <summary>
		/// Create the internal storage for the dbFile.
		/// Note:
		///   1. a helper file for segment managment b-tree is also created if this database file
		///      does not exist.
		/// </summary>
		/// <param name="dbFile"></param>
		public ObjectStore(string dbFileName)
		{
			m_dbFile = new DiskFile(dbFileName);
			m_indexFile = new DiskFile(dbFileName+".idx");
			m_indexPageManager = new memSegmentManager(m_indexFile);

			if (m_indexPageManager.Header.InitializeNeeded)
			{
				m_indexPageManager.Header.NextSegmentId = 0;
				m_indexPageManager.Header.NextObjectId = 0;
				m_indexPageManager.Header.NextClassId = 0;

				//initialize the segment b-tree's top node id
				OOD.Imp.Storage.BNode segTreeTop = new OOD.Imp.Storage.BNode();
				m_indexPageManager.GetNewSegment(segTreeTop);
				segTreeTop.SetOrder(m_segTreeNodeOrder);
				segTreeTop.Leaf = true;
				m_indexPageManager.Header.SegmentTreeTopNodeSId = segTreeTop.SegmentID;

				//initialize the space b-tree's top node id
				BNode spaceTreeTop = new BNode();
				m_indexPageManager.GetNewSegment(spaceTreeTop);
				spaceTreeTop.SetOrder(m_spaceTreeNodeOrder);
				spaceTreeTop.Leaf = true;
				m_indexPageManager.Header.FreeSpaceTreeTopNodeSId = spaceTreeTop.SegmentID;
			}

			m_segmentIndexTree = new SegTree(m_indexPageManager.Header.SegmentTreeTopNodeSId, m_indexPageManager);
			m_spaceIndexTree = new SpaceTree(m_indexPageManager.Header.FreeSpaceTreeTopNodeSId, m_indexPageManager, m_dbFile);

			m_dbSegmentManager = new dbSegmentManager(
				m_indexPageManager.Header.NextSegmentId,
				m_segmentIndexTree,
				m_spaceIndexTree,
				m_dbFile);


			if (m_indexPageManager.Header.InitializeNeeded)
			{
				BNode catalogTop = new BNode();
				m_dbSegmentManager.GetNewSegment(catalogTop);
				catalogTop.SetOrder(m_catalogTreeNodeOrder);
				catalogTop.Leaf = true;

				m_indexPageManager.Header.CatalogTreeTopNodeSId = catalogTop.SegmentID;
			}

			m_catalogTree = new CatalogTree(
				m_indexPageManager.Header.CatalogTreeTopNodeSId,
				m_dbSegmentManager,
				m_indexPageManager.Header.NextClassId);

			m_classCache = new LRUHashtable(m_cache_size, m_classCacheLimit);
			m_objectTreeCache = new LRUHashtable(m_cache_size, m_objectTreeCacheLimit);

			//bring top node's segment tree, space tree, catalog tree into cache, they will be needed anyway
			if (!m_indexPageManager.Header.InitializeNeeded)
			{
				m_indexPageManager.GetSegment(m_segmentIndexTree.TopNodSegId, new BNode(), new KSegId());
				m_indexPageManager.GetSegment(m_spaceIndexTree.TopNodeSId, new BNode(), new KOffset());
				m_dbSegmentManager.GetSegment(m_catalogTree.TopNodeSId, new BNode(), new KCatalog());
			}
		}

		public void Flush()
		{
			m_dbSegmentManager.Flush();

			m_indexPageManager.Header.CatalogTreeTopNodeSId = m_catalogTree.TopNodeSId;
			m_indexPageManager.Header.FreeSpaceTreeTopNodeSId = m_spaceIndexTree.TopNodeSId;
			m_indexPageManager.Header.NextClassId = m_catalogTree.NextCId;
			m_indexPageManager.Header.NextSegmentId = m_dbSegmentManager.NextSegmentId;
			m_indexPageManager.Header.SegmentTreeTopNodeSId = m_segmentIndexTree.TopNodSegId;
            
			m_indexPageManager.Flush();

			m_dbFile.Flush();
			m_indexFile.Flush();
		}

		public void Close()
		{
			Flush();
			m_indexPageManager.SetNormalClosed();
			m_dbFile.Close();
			m_indexFile.Close();
		}

		#region Object operations
		/**** User related operations *****/
		public void Put(object persistentObject)
		{
			System.Type t = persistentObject.GetType();
			string primaryKeyName = _get_primary_key_name(t);

			ClassInfo schemeInfo = SearchClassInfo(t.FullName);
			uint cid = 0;
			uint topNodeSid = 0;
			if (schemeInfo == null)
			{
				//insert it first
				InsertClassIno(t.FullName, _get_other_fields_name(t, primaryKeyName), ref cid, ref topNodeSid);
				schemeInfo = new ClassInfo(cid, t.FullName, _get_other_fields_name(t, primaryKeyName), topNodeSid);
			}
			else
			{
				cid = schemeInfo.CId;
				topNodeSid = schemeInfo.TopNodeSId;
			}
			
			Debug.Assert(topNodeSid > 0);

			OOD.Imp.Storage.ObjectTree ot = _get_object_tree(topNodeSid);

			//get the primary key value
			object keyValue = new object();
			//get the serialzed all othe fieldValues
			string[] otherFields;

			_get_data_for_storage(persistentObject, primaryKeyName, schemeInfo.FieldNames, out keyValue, out otherFields);

			Debug.Assert(otherFields != null);

			if (ot.InsertObject(keyValue, otherFields) == false)
				throw new OOD.Exception.DuplicatedPrimaryKey(
					this,
					"primary key is dupicated, insertion failed.");
			if (topNodeSid != ot.TopNodeSId)
			{
				//update the top node segment id needed
				m_catalogTree.UpdateTopNodeSId(t.FullName, ot.TopNodeSId);
			}
		}

		public object SearchScalar(string query)
		{
			string typeName, primaryKeyName, primaryKeyStrValue;
			if (_query_process(query, out typeName, out primaryKeyName, out primaryKeyStrValue))
			{
				Type t = _get_type_by_name(typeName);
				ClassInfo schemeInfo = m_catalogTree.Search(t.FullName);
				if (schemeInfo != null)
				{
					object keyValue = Types.GetValue(primaryKeyStrValue, Types.GetInternalType(Types.GetClassFieldType(t, primaryKeyName)));

					OOD.Imp.Storage.ObjectTree ot = _get_object_tree(schemeInfo.TopNodeSId);
					ObjectInfo oInfo = ot.SearchObject(keyValue);
					if (oInfo != null)
					{
						object result = _get_object_from_storage(oInfo, primaryKeyName, t, schemeInfo);

						return result;
					}
					else
						return null;
				}
				else 
					return null;
			}
			else
				return null;
		}
		#endregion

		#region class scheme information
		/* operations regarding class information, scheme */
		public bool InsertClassIno(string className, string[] fieldNames, ref uint assignedCId, ref uint topNodeSId)
		{
			if (m_catalogTree.Search(className) != null)
				return false;
			//reserve a segment id for this class clustering's top node
			BNode classTopSid = new BNode();
			m_dbSegmentManager.GetNewSegment(classTopSid);
			classTopSid.SetOrder(m_classTreeNodeOrder);
			classTopSid.Leaf = true;
			topNodeSId = classTopSid.SegmentID;
			if (m_catalogTree.Insert(className, fieldNames, classTopSid.SegmentID, ref assignedCId) == false)
				throw new OOD.Exception.ProgramLogicError(
					this,
					"Inserting class information failed.");
			return true;
		}

		public ClassInfo SearchClassInfo(string className)
		{
			return (ClassInfo)m_catalogTree.Search(className);
		}

#if DEBUG
		//we don't need to delete class info even if there is not object stored there
		//just for debug purpose to make sure the b-tree deletion works fine with the segement management
		public bool DeleteClassInfo(string className)
		{
			return m_catalogTree.Delete(className);
		}
#endif
		#endregion

		/// <summary>
		/// Get all other fileds name other than the primary key.
		/// </summary>
		/// <param name="t"></param>
		/// <param name="primaryKey"></param>
		/// <returns></returns>
		public string[] _get_other_fields_name(System.Type t, string primaryKey)
		{
			FieldInfo[] fields = t.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
			string[] result = new string[fields.Length-1];
			int pos = 0;
			for (int i=0; i<fields.Length; i++)
			{
				if (fields[i].Name != primaryKey)
					result[pos++] = fields[i].Name;
			}

			Debug.Assert(pos == result.Length);

			return result;
		}

		//make sure the class has the proper declaration.
		public string _get_primary_key_name(System.Type t)
		{
			object[] attributes = t.GetCustomAttributes(typeof(OOD.SchemaDefine),true);
			if (attributes != null && attributes.Length == 1)
			{
				SchemaDefine declared = (SchemaDefine)attributes[0];
				string pk = declared.PrimaryKey;
				FieldInfo pkField = t.GetField(pk, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
				if (pkField == null)
					throw new OOD.Exception.MissingPrimaryKey(
						this,
						"Primary key field is not found.");

				if (pkField.IsStatic || pkField.IsInitOnly)
					throw new OOD.Exception.MissingPrimaryKey(
						this,
						"Primary key can not defined on the static field member or initialize only field.");

				return pk;
			}
			else
				throw new OOD.Exception.MissingPrimaryKey(
					this,
					"Primary key is not defined.");

		}

		public void _get_data_for_storage(object obj, string primaryKeyName, string[] otherFieldsName, out object primaryKeyValue, out string[] otherFields)
		{
			System.Type tp = obj.GetType();
			primaryKeyValue = tp.InvokeMember(
				primaryKeyName,
				BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance,
				null,
				obj,
				null);

			otherFields = new string[otherFieldsName.Length];
			for (int i=0; i<otherFieldsName.Length; i++)
			{
	//			if (
				otherFields[i] = tp.InvokeMember(
					otherFieldsName[i],
					BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance,
					null,
					obj,
					null).ToString();

			}

		}

		public object _get_object_from_storage(ObjectInfo back, string primaryKeyName, Type tp, ClassInfo cinfo)
		{
			object result = tp.InvokeMember(null,
				BindingFlags.Public | BindingFlags.NonPublic |
				BindingFlags.Instance | BindingFlags.CreateInstance, null, null, null);
			//put the primary key back
			tp.InvokeMember(
				primaryKeyName,
				BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.SetField | BindingFlags.Instance,
				null,
				result,
				new object[] { back.PrimaryKeyValue });
			//put other fields back
			for (int i=0; i< cinfo.FieldNames.Length; i++)
			{
				FieldInfo field = tp.GetField(cinfo.FieldNames[i], BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				
				tp.InvokeMember(
					cinfo.FieldNames[i],
					BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.SetField | BindingFlags.Instance,
					null,
					result,
					new object[] { Types.GetValue(back.SerializedOtherFieldValues[i], Types.GetInternalType(field.FieldType)) });
			}

            return result;
		}

		public bool _query_process(string query, out string typeName, out string primaryKeyName, out string primaryKeyStrValue)
		{
			typeName = "";
			primaryKeyName = "";
			primaryKeyStrValue = "";

			Regex regQuery = new Regex(
				"select\\s*(?<typeName>[^\\s]+)\\s*where\\s*(?<primary>[^\\s]+)\\s*=\\s*(?<value>[^\\s]+)",
				RegexOptions.Compiled | RegexOptions.IgnoreCase);
			Match matQuery = regQuery.Match(query);
			if (matQuery.Success)
			{
                typeName = matQuery.Groups["typeName"].ToString();
				primaryKeyName = matQuery.Groups["primary"].ToString();
				primaryKeyStrValue = matQuery.Groups["value"].ToString();

				return true;
			}
			else
				return false;
		}

		public Type _get_type_by_name(string typeName)
		{

			if (!m_classCache.Exists(typeName))
			{
				m_classCache.Insert(typeName, Types.FindTypeByName(typeName));
			}

			return (System.Type)m_classCache.Retrive(typeName);
		}

		public ObjectTree _get_object_tree(uint topNodeSId)
		{
			if (!m_objectTreeCache.Exists(topNodeSId))
			{
				ObjectTree ot = new ObjectTree(topNodeSId, m_dbSegmentManager);
				m_objectTreeCache.Insert(topNodeSId, ot);
			}
			return (ObjectTree)m_objectTreeCache.Retrive(topNodeSId);
		}
	}
}
