using System;

namespace OOD.Imp.Storage
{
	/// <summary>
	/// The key in the segment managment b-tree.
	/// </summary>
	/// <MaxLength>8 + 4 + 2 = 14 bytes</MaxLength>
	public class KSegId : IKey
	{
		private		uint		m_id;
		private		DSegAddr	m_addr;

		public uint ID
		{
			get { return m_id;}
			set { m_id = value;}
		}

		public KSegId()
		{
		}

		public KSegId(uint id)
		{
			m_id = id;
			m_addr = null;
		}

		public KSegId(uint id, DSegAddr addr)
		{
			m_id = id;
			m_addr = addr;
		}

		public override string ToString()
		{
			if (m_addr == null)
				return m_id.ToString();
			else
			{
				return string.Format("{0}({1},{2})",m_id, m_addr.Offset, m_addr.Length);
			}
		}


		public DSegAddr Addr
		{
			get { return this.m_addr;}
			set { this.m_addr = value;}
		}

		#region IKey Members

		public IData Data
		{
			get
			{
				return m_addr;
			}
			set
			{
				m_addr = (DSegAddr)value;
			}
		}


		public int CompareTo(IKey B)
		{
			KSegId b = (KSegId)B;
			return (int)(m_id - b.m_id);
		}

		/*  Store format:
		 *    [m_id]([addr length][m_addr]) -- () means optional
		 *      4         2          x
		 */
		public byte[] Serialize()
		{
			byte[] result = null;
			if (m_addr == null)
			{
				result = new byte[4];
				OOD.Utility.Bytes.Pack4U(result, 0, m_id);
			}
			else
			{
				byte[] addr = m_addr.Serialize();
				result = new byte[addr.Length+4+2];
				OOD.Utility.Bytes.Pack4U(result, 0, m_id);
				OOD.Utility.Bytes.Pack2U(result, 4, (ushort)addr.Length);
				Array.Copy(addr, 0, result, 6, addr.Length);
			}
			return result;
		}

		public IKey Deserialize(byte[] bytes)
		{
			return Deserialize(bytes, 0, bytes.Length);
		}

		public IKey Deserialize(byte[] bytes, int offset, int count)
		{
			KSegId result = null;
			int pos = offset;
			uint id = OOD.Utility.Bytes.Unpack4U(bytes, pos);
			pos += 4;
			if (count == 4)
			{
				result = new KSegId(id);
			}
			else
			{
				int length = OOD.Utility.Bytes.Unpack2(bytes, pos);
				pos += 2;
				DSegAddr addr = new DSegAddr();
				addr = (DSegAddr)addr.Deserialize(bytes, pos, length);
				result = new KSegId(id, addr);
			}

			return result;
		}

		#endregion
	}
}
