using System;

namespace OOD.Imp.Storage
{
	/// <summary>
	/// The keys in the B-tree have to implement this interface.
	/// </summary>
	public interface IKey
	{
		IData Data
		{
			get; set;
		}


        int CompareTo(IKey B); //compare this with another key, 0: this==B, >0: this>B, <0: this<b
		byte[] Serialize(); //seralize the key, including the data field, into the byte array
		IKey Deserialize(byte[] bytes);
		IKey Deserialize(byte[] bytes, int offset, int count);

	}
}
