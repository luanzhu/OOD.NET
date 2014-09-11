using System;

namespace OOD.Imp.Storage
{
	/// <summary>
	/// event handler for B-tree key index Node changed event
	/// </summary>
	/// <remarks>
	/// segmentId is the new segment Id for the key, indexSlot is the new indexSlot for the key.
	/// </remarks>
	public delegate void KeySegmentChangedEventHandler(IKey posChangedKey, int segmentId);

}
