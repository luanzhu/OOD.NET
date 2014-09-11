using System;
using System.Reflection;

namespace OOD
{
	/// <summary>
	/// the supported types in the database system.
	/// </summary>
	public class Types
	{
		public Types()
		{
		}

		public enum InternalType 
		{
			t_Boolean,
			t_Byte,
			t_SByte,
			t_Short, 
			t_UShort,
			t_Char,
			t_Enum,
			t_Int,
			t_UInt,
			t_Long,
			t_ULong,
			t_Float,
			t_Double,
			t_String,
			t_Date,
			t_Object,
			t_Value,
			t_ArrayOfBoolean,
			t_ArrayOfByte,
			t_ArrayOfSByte,
			t_ArrayOfShort, 
			t_ArrayOfUShort,
			t_ArrayOfChar,
			t_ArrayOfEnum,
			t_ArrayOfInt,
			t_ArrayOfUInt,
			t_ArrayOfLong,
			t_ArrayOfULong,
			t_ArrayOfFloat,
			t_ArrayOfDouble,
			t_ArrayOfString,
			t_ArrayOfDate,
			t_ArrayOfObject,
			t_ArrayOfValue,

			t_Unsupported
		}

		public static InternalType GetInternalType(Type t)
		{
			InternalType type;
			if (t.Equals(typeof(byte)))
			{
				type = InternalType.t_Byte;
			}
			else if (t.Equals(typeof(sbyte)))
			{
				type = InternalType.t_SByte;
			}
			else if (t.Equals(typeof(short)))
			{
				type = InternalType.t_Short;
			}
			else if (t.Equals(typeof(ushort)))
			{
				type = InternalType.t_UShort;
			}
			else if (t.Equals(typeof(char)))
			{
				type = InternalType.t_Char;
			}
			else if (t.Equals(typeof(int)))
			{
				type = InternalType.t_Int;
			}
			else if (t.Equals(typeof(uint)))
			{
				type = InternalType.t_UInt;
			}
			else if (t.Equals(typeof(long)))
			{
				type = InternalType.t_Long;
			}
			else if (t.Equals(typeof(ulong)))
			{
				type = InternalType.t_ULong;
			}
			else if (t.Equals(typeof(float)))
			{
				type = InternalType.t_Float;
			}
			else if (t.Equals(typeof(double)))
			{
				type = InternalType.t_Double;
			}
			else if (t.Equals(typeof(System.String)))
			{
				type = InternalType.t_String;
			}
			else if (t.Equals(typeof(bool)))
			{
				type = InternalType.t_Boolean;
			}
			else if (t.Equals(typeof(System.DateTime)))
			{
				type = InternalType.t_Date;
			}
			else if (t.IsEnum) 
			{ 
				type = InternalType.t_Enum;
			}
			else if (typeof(object).IsAssignableFrom(t))
			{
				type = InternalType.t_Object;
			}
			else if (typeof(ValueType).IsAssignableFrom(t))
			{
				type = InternalType.t_Value;
			}
			else if (t.IsArray)
			{
				type = GetInternalType(t.GetElementType());
				if ((int)type >= (int)InternalType.t_Value)
				{
					return InternalType.t_Unsupported;
				}
				type = (InternalType)((int)type + (int)InternalType.t_ArrayOfBoolean);
			}
			else
			{
				type = InternalType.t_Unsupported;
			}
			return type;
		}

		public static System.Type GetSystemTypeSimple(InternalType t)
		{
			switch (t)
			{
				case InternalType.t_Boolean:
					return typeof(bool);
				case InternalType.t_Byte:
					return typeof(byte);
				case InternalType.t_SByte:
					return typeof(sbyte);
				case InternalType.t_Short:
					return typeof(short);
				case InternalType.t_UShort:
					return typeof(ushort);
				case InternalType.t_Char:
					return typeof(char);
				case InternalType.t_Enum:
					return null;
				case InternalType.t_Int:
					return typeof(int);
				case InternalType.t_UInt:
					return typeof(uint);
				case InternalType.t_Long:
					return typeof(long);
				case InternalType.t_ULong:
					return typeof(ulong);
				case InternalType.t_Float:
					return typeof(float);
				case InternalType.t_Double:
					return typeof(double);
				case InternalType.t_String:
					return typeof(string);
				case InternalType.t_Date:
					return typeof(System.DateTime);
				case InternalType.t_Object:
				case InternalType.t_Value:
				case InternalType.t_ArrayOfBoolean:
				case InternalType.t_ArrayOfByte:
				case InternalType.t_ArrayOfSByte:
				case InternalType.t_ArrayOfShort:
				case InternalType.t_ArrayOfUShort:
				case InternalType.t_ArrayOfChar:
				case InternalType.t_ArrayOfEnum:
				case InternalType.t_ArrayOfInt:
				case InternalType.t_ArrayOfUInt:
				case InternalType.t_ArrayOfLong:
				case InternalType.t_ArrayOfULong:
				case InternalType.t_ArrayOfFloat:
				case InternalType.t_ArrayOfDouble:
				case InternalType.t_ArrayOfString:
				case InternalType.t_ArrayOfDate:
				case InternalType.t_ArrayOfObject:
				case InternalType.t_ArrayOfValue:
				case InternalType.t_Unsupported:
					return null;
			}

			return null;
		}

		/// <summary>
		/// Convert the val to a byte array.
		/// </summary>
		/// <param name="val"></param>
		/// <param name="tp"></param>
		/// <returns></returns>
		public static byte[] GetByteArray(object val, InternalType tp)
		{
			int offset = 0;
			byte[] result = null;
			switch (tp)
			{
				case InternalType.t_Boolean:
					return BitConverter.GetBytes((bool)val);
				case InternalType.t_Byte:
					return new byte[] { (byte)val };
				case InternalType.t_SByte:
					return BitConverter.GetBytes((byte)val);
				case InternalType.t_Short:
					return BitConverter.GetBytes((short)val);
				case InternalType.t_UShort:
					return BitConverter.GetBytes((ushort)val);
				case InternalType.t_Char:
					return BitConverter.GetBytes((char)val);
				case InternalType.t_Enum:
					return BitConverter.GetBytes((short)val);
				case InternalType.t_Int:
					return BitConverter.GetBytes((int)val);
				case InternalType.t_UInt:
					return BitConverter.GetBytes((uint)val);
				case InternalType.t_Long:
					return BitConverter.GetBytes((long)val);
				case InternalType.t_ULong:
					return BitConverter.GetBytes((ulong)val);
				case InternalType.t_Float:
					return BitConverter.GetBytes((float)val);
				case InternalType.t_Double:
					return BitConverter.GetBytes((double)val);
				case InternalType.t_String:
					return System.Text.ASCIIEncoding.ASCII.GetBytes((string)val);
				case InternalType.t_Date:
					long t_ticks = ((DateTime)val).Ticks;
					return BitConverter.GetBytes(t_ticks);
				/* FORMAT for array */
				// [Lenght] [xx] [xxx] [xx]
				//   2 bytes
//				case InternalType.t_ArrayOfBoolean:
//					bool[] t_b_array = (bool[])val;
//					result = new byte[t_b_array.Length+2];
//					for (int i=0; i<t_b_array.Length; i++)
//					{
//						byte[] t = BitConverter.GetBytes(t_b_array[i]);
//						result[i] = t[0];
//					}
//					return result;
//				case InternalType.t_ArrayOfByte:
//					return (byte[])val;
//				case InternalType.t_ArrayOfSByte:
//					sbyte[] t_sb_in = (sbyte[])val;
//					byte[] t_b_out = new byte[t_sb_in.Length];
//					for (int i=0; i<t_sb_in.Length; i++)
//						t_b_out[i] = (byte)t_sb_in[i];
//					return t_b_out;
//				case InternalType.t_ArrayOfShort:
//					short[] t_short_in = (short[])val;
//					byte[] t_short_out = new byte[t_short_in.Length<<1];
//					int short_offset = 0;
//					for (int i=0; i<t_short_in.Length; i++)
//					{
//						Utility.Bytes.Pack2(t_short_out, short_offset, t_short_in[i]);
//						short_offset += 2;
//					}
//					return t_short_out;
//				case InternalType.t_ArrayOfUShort:
//					ushort[] t_ushort_in = (ushort[])val;
//					byte[] t_ushort_out = new byte[t_ushort_in.Length<<1];
//					int ushort_offset = 0;
//					for (int i=0; i<t_ushort_in.Length; i++)
//					{
//						Utility.Bytes.Pack2U(t_ushort_out, ushort_offset, t_ushort_in[i]);
//						ushort_offset += 2;
//					}
//					return t_ushort_out;
//				case InternalType.t_ArrayOfEnum:
//					System.Array t_enum_in = (System.Array)val;
//					byte[] t_enum_out = new byte[t_enum_in.Length<<1];
//					int enum_offset = 0;
//					for (int i=0; i<t_enum_in.Length; i++)
//					{
//						Utility.Bytes.Pack2(t_enum_out, enum_offset, (short)t_enum_in.GetValue(i));
//						enum_offset += 2;
//					}
//					return t_enum_out;
//				case InternalType.t_ArrayOfInt:
//					int[] t_int_in = (int[]) val;
//					byte[] t_int_out = new byte[t_int_in.Length << 2];
//					int int_offset = 0;
//					for (int i=0; i<t_int_in.Length; i++)
//					{
//						Utility.Bytes.Pack4(t_int_out, int_offset, t_int_in[i]);
//						int_offset += 4;
//					}
//					return t_int_out;
//				case InternalType.t_ArrayOfUInt:
//					uint[] t_uint_in = (uint[])val;
//					byte[] t_uint_out = new byte[t_uint_in.Length << 2];
//					int uint_offset = 0;
//					for (int i=0; i<t_uint_in.Length; i++)
//					{
//						Utility.Bytes.Pack4U(t_uint_out, uint_offset, t_uint_in[i]);
//						uint_offset += 4;
//					}
//					return t_uint_out;
//				case InternalType.t_ArrayOfLong:
//					long[] t_long_in = (long[])val;
//					byte[] t_long_out = new byte[t_long_in.Length << 3];
//					int long_offset = 0;
//					for (int i=0; i<t_long_in.Length; i++)
//					{
//						Utility.Bytes.Pack8(t_long_out, long_offset, t_long_in[i]);
//						long_offset += 8;
//					}
//					return t_long_out;
//				case InternalType.t_ArrayOfULong:
//					ulong[] t_ulong_in = (ulong[])val;
//					byte[] t_ulong_out = new byte[t_ulong_in.Length << 3];
//					int ulong_offset = 0;
//					for (int i=0; i<t_ulong_in.Length; i++)
//					{
//						Utility.Bytes.Pack8(t_ulong_out, ulong_offset, t_ulong_in[i]);
//						ulong_offset += 8;
//					}
//					return t_ulong_out;
//				case InternalType.t_ArrayOfFloat: //4 bytes
//					float[] t_float_in = (float[])val;
//					byte[] t_float_out = new byte[t_float_in.Length << 2];
//					int float_offset = 0;
//					for (int i=0; i<t_float_in.Length; i++)
//					{
//						byte[] bf = BitConverter.GetBytes(t_float_in[i]);
//						Array.Copy(bf, 0, t_float_out, float_offset, 4);
//						float_offset += 4;
//					}
//					return t_float_out;                    
//				case InternalType.t_ArrayOfDouble: //8 bytes
//					float[] t_float_in = (float[])val;
//					byte[] t_float_out = new byte[t_float_in.Length << 2];
//					int float_offset = 0;
//					for (int i=0; i<t_float_in.Length; i++)
//					{
//						byte[] bf = BitConverter.GetBytes(t_float_in[i]);
//						Array.Copy(bf, 0, t_float_out, float_offset, 4);
//						float_offset += 4;
//					}
//					return t_float_out;                    


				case InternalType.t_ArrayOfDate:
				case InternalType.t_ArrayOfString: //array of string is a little bit different
					return null;
				case InternalType.t_ArrayOfChar: //array of char is a little bit different
					return null;



					/*
								InternalType.t_ArrayOfObject,
								InternalType.t_ArrayOfValue,
					*/
				case InternalType.t_Unsupported:
					throw new OOD.Exception.NotImplemented(
						null, "This data type is not supported yet.");
			
			}

			return null;

		}


		public static object GetValue(string strValue, InternalType tp)
		{
			char[] splitor = new char[] { ';' };
			string[] temp_array = null;
			System.Array array_result = null;

			switch (tp)
			{
				case InternalType.t_Boolean:
					return Convert.ToBoolean(strValue);
				case InternalType.t_Byte:
					return Convert.ToByte(strValue);
				case InternalType.t_SByte:
					return Convert.ToSByte(strValue);
				case InternalType.t_Short:
					return Convert.ToInt16(strValue);
				case InternalType.t_UShort:
					return Convert.ToUInt16(strValue);
				case InternalType.t_Char:
					return Convert.ToChar(strValue);
				case InternalType.t_Enum:
					return Convert.ToInt32(strValue);
				case InternalType.t_Int:
					return Convert.ToInt32(strValue);
				case InternalType.t_UInt:
					return Convert.ToUInt32(strValue);
				case InternalType.t_Long:
					return Convert.ToInt64(strValue);
				case InternalType.t_ULong:
					return Convert.ToUInt64(strValue);
				case InternalType.t_Float:
					return Convert.ToSingle(strValue);
				case InternalType.t_Double:
					return Convert.ToDouble(strValue);
				case InternalType.t_String:
					return strValue;
				case InternalType.t_Date:
					return Convert.ToDateTime(strValue);
/* unsupported right now
			InternalType.t_Object,
			InternalType.t_Value,
*/			
				case InternalType.t_ArrayOfBoolean:			
				case InternalType.t_ArrayOfByte:
				case InternalType.t_ArrayOfSByte:
				case InternalType.t_ArrayOfShort:
				case InternalType.t_ArrayOfUShort:
				case InternalType.t_ArrayOfEnum:
				case InternalType.t_ArrayOfInt:
				case InternalType.t_ArrayOfUInt:
				case InternalType.t_ArrayOfLong:
				case InternalType.t_ArrayOfULong:
				case InternalType.t_ArrayOfFloat:
				case InternalType.t_ArrayOfDouble:
				case InternalType.t_ArrayOfDate:
					temp_array = strValue.Split(splitor);
					array_result = System.Array.CreateInstance(GetSystemTypeSimple(tp), temp_array.Length);
					for (int i=0; i<temp_array.Length; i++)
						array_result.SetValue(GetValue(temp_array[i], InternalType.t_Boolean), i);
					return array_result;

				case InternalType.t_ArrayOfString: //array of string is a little bit different
					return null;
				case InternalType.t_ArrayOfChar: //array of char is a little bit different
					return null;



/*
			InternalType.t_ArrayOfObject,
			InternalType.t_ArrayOfValue,
*/
				case InternalType.t_Unsupported:
					throw new OOD.Exception.NotImplemented(
						null, "This data type is not supported yet.");
			
			}

			return null;
		}


		public static System.Type FindTypeByName(string name)
		{
			Type t = null;

			foreach (Assembly asse in AppDomain.CurrentDomain.GetAssemblies()) 
			{ 
				foreach (Module mod in asse.GetModules()) 
				{ 
					foreach (Type temp in mod.GetTypes())
					{ 
//						if (t != null) 
//						{ 
//							throw new OOD.Exception.SchemeDefinition(
//								null,
//								"Ambingous type defined.");
//
//						} 
						if (temp.FullName == name)
						{ 
							return temp;
						}
					}
				}
			}

			if (t == null) 
			{
				throw new OOD.Exception.SchemeDefinition(
					null,
					"Stored type is not defined in the program.");
			}
			
			return t;
			
		}

		public static Type GetClassFieldType(Type t, string fieldName)
		{
			FieldInfo field = t.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance);
			return field.FieldType;
		}
	}
}
