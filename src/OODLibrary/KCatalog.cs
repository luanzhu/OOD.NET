using System;

namespace OOD.Imp.Storage
{
	/// <summary>
	/// The key for the system catalog b-tree.
	/// </summary>
	public class KCatalog : IKey
	{
		private			string			m_className;
		private			DCatalog		m_info;

		public KCatalog()
		{
		}

		public KCatalog(string className)
		{
			m_className = className;
		}

		public DCatalog ClassInfo
		{
			get { return m_info;}
			set { m_info =value;}
		}

		public string ClassName
		{
			get { return m_className;}
			set { m_className = value;}
		}

		public override string ToString()
		{
			if (m_info != null)
				return string.Format("{0}({1})", m_className, m_info.CId);
			else
				return m_className;
		}


		#region IKey Members

		public IData Data
		{
			get
			{
				return m_info;
			}
			set
			{
				m_info = (DCatalog)value;
			}
		}


		public int CompareTo(IKey B)
		{
			KCatalog kb = (KCatalog)B;
			return string.Compare(this.m_className, kb.m_className, false, System.Globalization.CultureInfo.InvariantCulture);
		}

		public byte[] Serialize()
		{
			if (m_info == null)
			{
				short len = (short)m_className.Length;
				byte[] result = new byte[len+2];
				OOD.Utility.Bytes.Pack2(result, 0, len);
				System.Text.ASCIIEncoding.ASCII.GetBytes(m_className, 0, len, result, 2);

				return result;
			}
			else
			{
				short len = (short)m_className.Length;
				byte[] info = m_info.Serialize();
				byte[] result = new byte[info.Length+len+2];
				int pos = 0;
				OOD.Utility.Bytes.Pack2(result, pos, len);
				pos += 2;
				System.Text.ASCIIEncoding.ASCII.GetBytes(m_className, 0, len, result, pos);
				pos += len;

				Array.Copy(info, 0, result, pos, info.Length);

				return result;
			}
		}

		public IKey Deserialize(byte[] bytes, int offset, int count)
		{
			int pos = offset;
			KCatalog result = new KCatalog();
			short len = OOD.Utility.Bytes.Unpack2(bytes, pos);
			pos += 2;
			result.m_className = System.Text.ASCIIEncoding.ASCII.GetString(bytes, pos, len);
			pos += len;

			if (count > len + 2)
			{
				DCatalog catFactory = new DCatalog();
				result.m_info = (DCatalog)catFactory.Deserialize(bytes, pos, count - len - 2);
			}

			return result;
		}


		public IKey Deserialize(byte[] bytes)
		{
			return Deserialize(bytes, 0, bytes.Length);
		}


		#endregion
	}
}
