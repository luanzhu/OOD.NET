using System;
using System.Diagnostics;

namespace OOD.Imp.Storage
{
	/// <summary>
	/// The data field of the key in catalog b-tree.
	/// </summary>
	public class DCatalog : IData
	{
		private			uint			m_topNodeSId; //the segment id of the top node of the clustering b-tree.
		private			string[]		m_fields; //the fields name array
		private			uint			m_cid;

		public DCatalog()
		{
		}

		public DCatalog(uint cid, string[] fields, uint topNodeSId)
		{
			m_cid = cid;
			m_fields = fields;
			m_topNodeSId = topNodeSId;
		}

		public uint	CId
		{
			get { return m_cid;}
			set { m_cid = value;}
		}

		public uint TopNodeSId
		{
			get { return m_topNodeSId;}
			set { m_topNodeSId = value;}
		}

		public string[] FieldNames
		{
			get { return m_fields;}
			set { this.m_fields = value;}
		}

		/// <summary>
		/// Given a fields name, return the its field id.
		/// </summary>
		/// <param name="?"></param>
		/// <returns></returns>
		public int GetFieldId(string fieldName)
		{
			for (int i=0; i<m_fields.Length; i++)
				if (m_fields[i] == fieldName)
					return i;
			return -1;
		}

		/// <summary>
		/// Given a field ID, return its field name.
		/// </summary>
		/// <param name="fieldId"></param>
		/// <returns></returns>
		public string GetFieldName(int fieldId)
		{
			Debug.Assert(fieldId>0 && fieldId < m_fields.Length);
			return m_fields[fieldId];
		}

		#region IData Members

		public byte[] Serialize()
		{
			//count the bytes needed
			int length = 4 + 2 + 4;
			for (int i=0; i<m_fields.Length; i++)
			{
				length += m_fields[i].Length + 2;
			}
			byte[] result = new byte[length];
			int pos = 0;
			OOD.Utility.Bytes.Pack4U(result, pos, m_topNodeSId);
			pos += 4;

			OOD.Utility.Bytes.Pack4U(result, pos, m_cid);
			pos += 4;

			//put fields array
			OOD.Utility.Bytes.Pack2(result, pos, (short)m_fields.Length);
			pos += 2;
			for (int i=0; i<m_fields.Length; i++)
			{
				short len = (short)m_fields[i].Length;

				Debug.Assert(len >0);

				OOD.Utility.Bytes.Pack2(result, pos, len);
				pos += 2;
				System.Text.ASCIIEncoding.ASCII.GetBytes(m_fields[i],0,len,result, pos);
				pos += len;
			}
			
			Debug.Assert(pos == length);
			
			return result;
		}

		public IData Deserialize(byte[] bytes, int offset, int count)
		{
			int pos = offset;
			DCatalog result = new DCatalog();
			result.m_topNodeSId = OOD.Utility.Bytes.Unpack4U(bytes, pos);
			pos += 4;

			result.m_cid = OOD.Utility.Bytes.Unpack4U(bytes, pos);
			pos += 4;

			int fNum = OOD.Utility.Bytes.Unpack2(bytes, pos);
			pos += 2;
			result.m_fields = new string[fNum];
			for (int i=0; i<fNum; i++)
			{
				int len = OOD.Utility.Bytes.Unpack2(bytes, pos);
				pos += 2;
				result.m_fields[i] = System.Text.ASCIIEncoding.ASCII.GetString(bytes, pos, len);
				pos += len;

			}
            
			Debug.Assert(pos == offset + count);

			return result;
		}


		public IData Deserialize(byte[] bytes)
		{
			return Deserialize(bytes, 0, bytes.Length);
		}



		#endregion
	}
}
