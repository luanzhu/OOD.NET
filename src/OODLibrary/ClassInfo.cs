using System;
using System.Diagnostics;

namespace OOD
{
	public class ClassInfo
	{
		protected			uint				m_cid;
		protected			string				m_className;
		protected			uint				m_topNodeSId;
		protected			string[]			m_fields;

		public ClassInfo(uint cid, string className, string[] fields, uint topNodeSId)
		{
			m_cid = cid;
			m_className = className;
			m_topNodeSId = topNodeSId;
			m_fields = fields;
		}

		public ClassInfo()
		{
		}

		public uint	CId
		{
			get { return m_cid;}
		}

		public string ClassName
		{
			get { return m_className;}
		}

		public uint TopNodeSId
		{
			get {return m_topNodeSId;}
		}

		public string[] FieldNames
		{
			get { return m_fields;}
		}

		public int GetFieldId(string fieldName)
		{
			for (int i=0; i<m_fields.Length; i++)
				if (m_fields[i] == fieldName)
					return i;
			return -1;
		}

		public string GetFieldName(int fieldId)
		{
			Debug.Assert(fieldId >=0 && fieldId < m_fields.Length);
			return m_fields[fieldId];
		}

		public void SetTopNodeSId(uint topSId)
		{
			m_topNodeSId = topSId;
		}
	}
}
