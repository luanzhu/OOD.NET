using System;

namespace OOD.Imp.Storage
{
	/// <summary>
	/// the key for the b-tree to manage free segment space in the database file.
	/// </summary>
	/// <MaxLength>8 bytes</MaxLength>
	public class KOffset : IKey
	{
		private					uint			m_offset; //the offset in the file for the free space starts
		private					DLength			m_length;


		public KOffset()
		{
		}

		public KOffset(uint offset)
		{
			m_offset = offset;
		}

		public KOffset(uint offset, DLength length)
		{
			m_length = length;
		}

		public uint Offset
		{
			get { return m_offset;}
			set { m_offset = value;}
		}

		public DLength Length
		{
			get { return m_length;}
			set { m_length = value;}
		}

		public override string ToString()
		{
			if (m_length != null)
				return string.Format("({0},{1})", m_offset, m_length.Num);
			else
				return string.Format("({0},-)", m_offset);
		}

		#region IKey Members

		public IData Data
		{
			get
			{
				return m_length;
			}
			set
			{
				m_length = (DLength)value;
			}
		}


		public int CompareTo(IKey B)
		{
			KOffset bb = (KOffset)B;
			return (int)(this.m_offset - bb.m_offset);
		}

		public byte[] Serialize()
		{
			if (m_length != null)
			{
				byte[] lengths = m_length.Serialize();
				byte[] result = new byte[lengths.Length+4];
				OOD.Utility.Bytes.Pack4U(result, 0, m_offset);
				Array.Copy(lengths, 0, result, 4, lengths.Length);

				return result;
			}
			else
			{
				return BitConverter.GetBytes(m_offset);
			}
		}

		public IKey Deserialize(byte[] bytes)
		{
			return Deserialize(bytes, 0, bytes.Length);
		}

		public IKey Deserialize(byte[] bytes, int offset, int count)
		{
			KOffset result = new KOffset();
			result.m_offset = OOD.Utility.Bytes.Unpack4U(bytes, offset);

			DLength dFactory = new DLength();

			if (count > 4)
			{
				result.m_length = (DLength)dFactory.Deserialize(bytes, offset + 4, count - 4);
			}

			return result;
		}

		#endregion
	}
}
