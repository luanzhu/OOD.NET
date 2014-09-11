using System;

namespace OOD.Utility
{

	/// <summary>
	/// Some common used tools for byte operations.
	/// </summary>
	public class Bytes
	{
		/// <summary>
		/// convert i to 4 byte, and insert it bArray at the right location
		/// </summary>
		/// <param name="bArray"></param>
		/// <param name="offset"></param>
		/// <param name="i"></param>
		public static void Pack4(byte[] bArray, int offset, int val)
		{
			byte[] temp = BitConverter.GetBytes(val);
			Array.Copy(temp,0,bArray,offset,temp.Length);
		}

		public static int Unpack4(byte[] bArray, int offset)
		{
			return BitConverter.ToInt32(bArray, offset);	
		}

		public static void Pack4U(byte[] bArray, int offset, uint val)
		{
			byte[] temp = BitConverter.GetBytes(val);
			Array.Copy(temp,0,bArray,offset,temp.Length);
		}

		public static uint Unpack4U(byte[] bArray, int offset)
		{
			return BitConverter.ToUInt32(bArray, offset);
		}

		public static void Pack2U(byte[] bArray, int offset, ushort val)
		{
			byte[] temp = BitConverter.GetBytes(val);
			Array.Copy(temp,0,bArray,offset,temp.Length);
		}

		public static ushort Unpack2U(byte[] bArray, int offset)
		{
			return BitConverter.ToUInt16(bArray, offset);
		}

		public static void Pack2(byte[] bArray, int offset, short val)
		{
			byte[] temp = BitConverter.GetBytes(val);
			Array.Copy(temp,0,bArray,offset,temp.Length);
		}

		public static short Unpack2(byte[] bArray, int offset)
		{
			return BitConverter.ToInt16(bArray, offset);
		}

		public static void Pack8(byte[] bArray, int offset, long val)
		{
			byte[] temp = BitConverter.GetBytes(val);
			Array.Copy(temp,0,bArray,offset,temp.Length);
		}

		public static long Unpack8(byte[] bArray, int offset)
		{
			return BitConverter.ToInt64(bArray, offset);
		}

	}
}
