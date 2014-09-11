using System;
using System.Diagnostics;

namespace OOD.Imp.Storage
{
	/// <summary>
	/// The data part of the key for segment management
	/// </summary>
	/// <MaxLength>8 bytes</MaxLength>
	public class DSegAddr : IData
	{
		private		uint	m_offset;
		private		int		m_length;


		public DSegAddr()
		{
			m_offset = 0;
			m_length = 0;
		}

		public DSegAddr(uint offset, int length)
		{
			m_offset = offset;
			m_length = length;
		}


		public uint Offset
		{
			get { return this.m_offset;}
			set { this.m_offset = value;}
		}

		public int Length
		{
			get { return this.m_length;}
			set { this.m_length = value;}
		}

		#region IData Members

		public byte[] Serialize()
		{
			byte[] result = new byte[8];
			OOD.Utility.Bytes.Pack4U(result, 0, m_offset);
			OOD.Utility.Bytes.Pack4(result, 4, m_length);
			return result;
		}

		public IData Deserialize(byte[] bytes)
		{
			return Deserialize(bytes, 0, bytes.Length);
		}

		public IData Deserialize(byte[] bytes, int offset, int count)
		{
			Debug.Assert(count == 8);

			uint l_offset = OOD.Utility.Bytes.Unpack4U(bytes, offset);
			int length = OOD.Utility.Bytes.Unpack4(bytes, offset + 4);
			DSegAddr result = new DSegAddr(l_offset, length);
			return result;
		}

		#endregion
	}
}
