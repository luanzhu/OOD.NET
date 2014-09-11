using System;

namespace OOD.Imp.Storage
{
	/// <summary>
	/// base class for the segment manager. 
	/// Note:
	///   1. dbSegmentManager will be used to manager the segment usage in the database file.
	///   2. memSegmentManager will be used to manager the segment(page) usage for the b-tree 
	///      used for dbSegmentManager.
	/// </summary>
	public abstract class SegmentManager
	{
		public abstract Segment GetSegment(uint segId, Segment segFactory, object helper);
		public abstract void GetNewSegment(Segment seg);
		public abstract void FreeSegment(Segment seg);

		/// <summary>
		/// Write all cached segment/page back to disk.
		/// </summary>
		public abstract void Close();
	}
}
