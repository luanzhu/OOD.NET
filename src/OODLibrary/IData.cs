using System;

namespace OOD.Imp.Storage
{
	/// <summary>
	/// All the user data part of the b-tree must implement this interface.
	/// </summary>
	public interface IData
	{
		byte[] Serialize();
		IData Deserialize(byte[] bytes);
		IData Deserialize(byte[] bytes, int offset, int count);
	}
}
