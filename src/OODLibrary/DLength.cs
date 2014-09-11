using System;
using System.Diagnostics;

namespace OOD.Imp.Storage
{
	/// <summary>
	/// The data field of the key in the free space managment b-tree.
	/// </summary>
	/// <MaxLength>4 bytes</MaxLength>
	public class DLength : IData
	{
		private				int					m_length;

		public DLength()
		{
		}

		public DLength(int length)
		{
			m_length = length;
		}

		public int Num
		{
			get { return m_length;}
			set { m_length = value;}
		}

		#region IData Members

		public byte[] Serialize()
		{			
			return BitConverter.GetBytes(m_length);
		}

		public IData Deserialize(byte[] bytes)
		{
			return Deserialize(bytes, 0, bytes.Length);
		}

		public IData Deserialize(byte[] bytes, int offset, int count)
		{
			Debug.Assert(count >= 4);

			DLength result = new DLength();
			result.m_length = BitConverter.ToInt32(bytes, offset);		
			return result;
		}

		#endregion
	}
}
