using System;
using System.Diagnostics;
using OOD;


namespace OOD.Imp.Storage
{
	/// <summary>
	/// The key will be used to store objects of class in the clustering b-tree.
	/// </summary>
	public class KClass : IKey
	{
		private			OOD.Types.InternalType		m_type;
		private			object					    m_value;
		private			DClass					    m_object_data;

		#region contructors
		public KClass()
		{
		}

		public KClass(object kvalue, Types.InternalType tp)
		{
			m_type = tp;
			m_value = kvalue;
		}

		public KClass(byte k)
		{
			m_type = Types.InternalType.t_Byte;
			m_value = k;
		}

		public KClass(sbyte k)
		{
			m_type = Types.InternalType.t_SByte;
			m_value = k;
		}

		public KClass(short k)
		{
			m_type = Types.InternalType.t_Short;
			m_value = k;
		}

		public KClass(ushort k)
		{
			m_type = Types.InternalType.t_UShort;
			m_value = k;
		}

		public KClass(char k)
		{
			m_type = Types.InternalType.t_Char;
			m_value = k;
		}

		public KClass(int k)
		{
			m_type = Types.InternalType.t_Int;
			m_value = k;
		}

		public KClass(uint k)
		{
			m_type = Types.InternalType.t_UInt;
			m_value = k;
		}

		public KClass(long k)
		{
			m_type = Types.InternalType.t_Long;
			m_value = k;
		}

		public KClass(ulong k)
		{
			m_type = Types.InternalType.t_ULong;
			m_value = k;
		}

		public KClass(float k)
		{
			m_type = Types.InternalType.t_Float;
			m_value = k;
		}

		public KClass(double k)
		{
			m_type = Types.InternalType.t_Double;
			m_value = k;
		}

		public KClass(string k)
		{
			m_type = Types.InternalType.t_String;
			m_value = k;
		}
		#endregion

		public Types.InternalType DataType
		{
			get { return m_type;}
			set { m_type = value;}
		}

		public DClass SerializedOtherData
		{
			get { return m_object_data;}
			set { m_object_data = value;}
		}

		public object KValue
		{
			get { return m_value;}
			set 
			{ 
				m_value = value;
				m_type = OOD.Types.GetInternalType(m_value.GetType());
			}
		}


		#region IKey Members

		public IData Data
		{
			get
			{
				return m_object_data;
			}
			set
			{
				m_object_data = (DClass)value;
			}
		}

		public int CompareTo(IKey B)
		{
			KClass KB = (KClass)B;
			switch (this.m_type)
			{
				case Types.InternalType.t_Byte:
					return (byte)this.m_value - (byte)KB.m_value;
				case Types.InternalType.t_SByte:
					return (sbyte)this.m_value - (sbyte)KB.m_value;
				case Types.InternalType.t_Short:
					return (short)this.m_value - (short)KB.m_value;
				case Types.InternalType.t_UShort:
					return (ushort)this.m_value - (ushort)KB.m_value;
				case Types.InternalType.t_Char:
					return (char)this.m_value - (char)KB.m_value;
				case Types.InternalType.t_Int:
					return (int)this.m_value - (int)KB.m_value;
				case Types.InternalType.t_UInt:
					return (int)((uint)this.m_value - (uint)KB.m_value);
				case Types.InternalType.t_Long:
					return (int)((long)this.m_value - (long)KB.m_value);
				case Types.InternalType.t_ULong:
					return (int)((ulong)this.m_value - (ulong)KB.m_value);
				case Types.InternalType.t_Float:
					float tmp = (float)this.m_value - (float)KB.m_value;
					if (tmp >0)
						return 1;
					else if (tmp <0)
						return -1;
					else return 0;
				case Types.InternalType.t_Double:
					double d_tmp = (double)this.m_value - (double)KB.m_value;
					if (d_tmp > 0)
						return 1;
					else if (d_tmp < 0)
						return -1;
					else return 0;
				case Types.InternalType.t_String:
					return string.Compare((string)m_value, (string)KB.m_value, false, System.Globalization.CultureInfo.InvariantCulture);
			}

			throw new OOD.Exception.NotImplemented(
				this,
				"Unsupported type for primary key detected.");
		}

		public byte[] Serialize()
		{
			/* FORMAT:
			 *	  [type][value len][value]
			 *      1      2         xxx
			 */
			byte[] objectData = null;
			if (m_object_data != null)
			{
				objectData = m_object_data.Serialize();
			}


			string strValue = m_value.ToString();
			int length = 1 + 2 + strValue.Length + (objectData == null ? 0 : objectData.Length);

			byte[] result = new byte[length];
			int pos = 0;
			//type
			result[pos++] = (byte)m_type;
			
			OOD.Utility.Bytes.Pack2(result, pos, (short)strValue.Length);
			pos += 2;
			System.Text.ASCIIEncoding.ASCII.GetBytes(strValue, 0, strValue.Length, result, pos);
			pos += strValue.Length;


			if (m_object_data != null)
			{
				Array.Copy(objectData, 0, result, pos, objectData.Length);
				pos += objectData.Length;
			}

			Debug.Assert(pos == length);

			return result;
		}

		public IKey Deserialize(byte[] bytes, int offset, int count)
		{
			KClass result = new KClass();
			int pos = offset;

			result.m_type = (Types.InternalType)bytes[pos++];

			int len = OOD.Utility.Bytes.Unpack2(bytes, pos);
			pos += 2;
			string strValue = System.Text.ASCIIEncoding.ASCII.GetString(bytes, pos, len);
			pos +=len;

			switch (result.m_type)
			{
				case Types.InternalType.t_Byte:
					result.m_value = Convert.ToByte(strValue);
					break;
				case Types.InternalType.t_SByte:
					result.m_value = Convert.ToSByte(strValue);
					break;
				case Types.InternalType.t_Short:
					result.m_value = Convert.ToInt16(strValue);
					break;
				case Types.InternalType.t_UShort:
					result.m_value = Convert.ToUInt16(strValue);
					break;
				case Types.InternalType.t_Char:
					result.m_value = Convert.ToChar(strValue);
					break;
				case Types.InternalType.t_Int:
					result.m_value = Convert.ToInt32(strValue);
					break;
				case Types.InternalType.t_UInt:
					result.m_value = Convert.ToUInt32(strValue);
					break;
				case Types.InternalType.t_Long:
					result.m_value = Convert.ToInt64(strValue);
					break;
				case Types.InternalType.t_ULong:
					result.m_value = Convert.ToUInt64(strValue);
					break;
				case Types.InternalType.t_Float:
					result.m_value = Convert.ToSingle(strValue);
					break;
				case Types.InternalType.t_Double:
					result.m_value = Convert.ToDouble(strValue);
					break;
				case Types.InternalType.t_String:
					result.m_value = strValue;
					break;
			}

			if (count > pos - offset)
			{
				//IDATA is there too
				m_object_data = new DClass();
				result.m_object_data = (DClass)m_object_data.Deserialize(bytes, pos, count - (pos - offset));
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
