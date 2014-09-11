using System;

namespace OOD.Imp.Storage
{
	/// <summary>
	/// The b-tree will be used to manage the free segment in the database file.
	/// </summary>
	public class SpaceTree
	{
		private		BTree				m_tree;
		private		SegmentManager		m_sgManager;
		private		DiskFile			m_dbFile; //the file handler for main database file
		

		public SpaceTree(uint topSid, SegmentManager sgManager, DiskFile dbFile)
		{
			m_tree = new BTree(topSid, sgManager, new KOffset());
			m_sgManager = sgManager;
			m_dbFile = dbFile;
		}

		public uint TopNodeSId
		{
			get { return m_tree.TopNodSegId;}
		}

		/*  Needed operations:
		 *	  1. Given an offset and length, set that range to be free space.
		 *    2. Given an length, return the pair (offset, length) can be used.
		 *		 that range will be removed from free space tree before this 
		 *       operation	returns.
		 */ 

		/// <summary>
		/// Set the segment in the databse file to be free.
		/// </summary>
		/// <param name="offset"></param>
		/// <param name="length"></param>
		public void SetSegmentFree(uint offset, int length)
		{
			if (offset + length != m_dbFile.Length)
			{
				uint nextSeg = (uint)(offset + length);
				KOffset target = new KOffset(nextSeg);
				KOffset finding = (KOffset)m_tree.Search(target);
				if (finding == null)
				{
					//combine to next segment is not possible, just go ahead to insert this one
					target.Offset = offset;
					target.Length = new DLength(length);
					if (m_tree.Insert(target) == false)
						throw new OOD.Exception.ProgramLogicError(
							this,
							"Insert into free space b-tree failed.");
				}
				else
				{
					//combine these two free segment into a big one
					length += finding.Length.Num;
					target.Length = new DLength(length);
					target.Offset = offset;
					if (m_tree.Delete(finding) == false || m_tree.Insert(target) == false)
						throw new OOD.Exception.ProgramLogicError(
							this,
							"Delete free space failed.");
				
				}
			}
			else
			{
				//this is the last segment in the database file, just remove it from the file
				m_dbFile.SetLength(offset);
			}
		}


		/// <summary>
		/// Request a segment with size of length.
		/// </summary>
		/// <param name="length"></param>
		/// <returns>True if succeed, otherwise false.</returns>
		/// <preCondition></preCondition>
		/// <PostCondition>A free segment will offset as the starting place and length as size
		/// is cleared from this free space tree.</PostCondition>
		public bool RequireSegment(int length, ref uint offset)
		{
			//do the sequential search
			OOD.Imp.Storage.BTree.BTEnumerator enumerator = m_tree.GetEnumerator();
			KOffset finding = null;
//			while (enumerator.MoveNext())
//			{
//				KOffset temp = (KOffset)enumerator.Current;
//				if (temp != null && temp.Length.Num >= length)
//				{
//					finding = temp;
//					break;
//				}
//			}
			if (finding == null)
			{
				//no free space find in the b-tree, create a new segment after the file end
				offset = (uint)m_dbFile.Length;
				m_dbFile.SetLength(offset + length);
			}
			else
			{
				//find new space in the tree
				offset = finding.Offset;
				int len = finding.Length.Num;
				if (m_tree.Delete(finding) == false)
					throw new OOD.Exception.ProgramLogicError(
						this,
						"Delete free space failed.");

				if (finding.Length.Num > length)
				{
					//put the left back
					finding.Offset = (uint)(offset + length);
					finding.Length.Num = len - length;
					if (m_tree.Insert(finding) == false)
						throw new OOD.Exception.ProgramLogicError(
							this,
							"Insert free space failed.");
				}
			}

			return true;
		}

	}
}
