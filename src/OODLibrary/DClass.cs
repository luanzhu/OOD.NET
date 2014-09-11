using System;
using System.Diagnostics;

namespace OOD.Imp.Storage
{
	/// <summary>
	/// IData field of the class b-tree.
	/// </summary>
	public class DClass : IData
	{
		//stored all the serialized value of each fields, other than primary key field, of the object
		private				string[]				m_fieldValues;


		public DClass()
		{
		}

		public DClass(string[] serializedValues)
		{
			m_fieldValues = serializedValues;
		}

		public string[] SerializedValues
		{
			get { return m_fieldValues;}
			set { this.m_fieldValues = value;}
		}


		#region IData Members

		public byte[] Serialize()
		{
			/*	FORMAT:
			 *		[nums][len1][conent1]....		
			 * 
			 */
			int length =2;
			for (int i=0; i<m_fieldValues.Length; i++)
			{
				length += 2 + m_fieldValues[i].Length;
			}
			byte[] result = new byte[length];

			int pos = 0;
			OOD.Utility.Bytes.Pack2(result, pos, (short)m_fieldValues.Length);
			pos += 2;

			for (int i=0; i<m_fieldValues.Length; i++)
			{
				int len = m_fieldValues[i].Length;
				OOD.Utility.Bytes.Pack2(result, pos, (short)len);
				pos += 2;
				System.Text.ASCIIEncoding.ASCII.GetBytes(m_fieldValues[i], 0, len, result, pos);
				pos += len;
			}

			Debug.Assert(pos == length);

			return result;
		}

		public IData Deserialize(byte[] bytes, int offset, int count)
		{
			int pos = offset;
			DClass result = new DClass();
			int fNum = OOD.Utility.Bytes.Unpack2(bytes, pos);
			pos += 2;;

			Debug.Assert(fNum > 0);

			result.m_fieldValues = new string[fNum];
			for (int i=0; i<fNum; i++)
			{
				int len = OOD.Utility.Bytes.Unpack2(bytes, pos);
				pos += 2;
				result.m_fieldValues[i] = System.Text.ASCIIEncoding.ASCII.GetString(bytes, pos, len);
				pos += len;
			}

			Debug.Assert(pos - offset == count);

			return result;
		}


		public IData Deserialize(byte[] bytes)
		{
			return Deserialize(bytes, 0, bytes.Length);
		}

		#endregion
	}
}
