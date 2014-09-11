using System;
using System.Diagnostics;

namespace OOD.Imp.Storage
{
	/// <summary>
	/// The real structure to save object into the database file.
	/// </summary>
	public class ObjectTree
	{
		private			BTree			m_tree;

		public ObjectTree(uint topNodeSId, SegmentManager sgManager)
		{
			m_tree = new BTree(topNodeSId, sgManager, new KClass());
		}	


		public uint TopNodeSId
		{
			get { return m_tree.TopNodSegId;}
		}

		public bool InsertObject(object keyValue, string[] serializedOtherFields)
		{
			KClass key = new KClass(keyValue, OOD.Types.GetInternalType(keyValue.GetType()));
			key.SerializedOtherData = new DClass(serializedOtherFields);

			return m_tree.Insert(key);
		}

		public ObjectInfo SearchObject(object keyValue)
		{
			KClass key = new KClass(keyValue, OOD.Types.GetInternalType(keyValue.GetType()));
			KClass obj = (KClass) m_tree.Search(key);
			if (obj != null)
			{
				Debug.Assert(obj.SerializedOtherData != null);
				ObjectInfo result = new ObjectInfo(key.DataType, key.KValue, obj.SerializedOtherData.SerializedValues);

				return result;
			}
			else
				return null;
		}


		public bool UpdateObject(object keyValue, string[] newSerializedOtherFields)
		{
			KClass target = new KClass(keyValue, OOD.Types.GetInternalType(keyValue.GetType()));
			target.SerializedOtherData = new DClass(newSerializedOtherFields);
			return m_tree.UpdateData(target);
		}

		public bool DeleteObject(object keyValue)
		{
			KClass target = new KClass(keyValue, OOD.Types.GetInternalType(keyValue.GetType()));
			return m_tree.Delete(target);
		}
	}
}
