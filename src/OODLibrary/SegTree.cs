using System;

namespace OOD.Imp.Storage
{
	/// <summary>
	/// The b-tree for segment managment.
	/// </summary>
	public class SegTree
	{
		/* Needed operation
		 *	1. create a segment manager b-tree given a top node
		 *  2. given a segment id, get its offset and length
		 *  3. insert a segment information, segment id, offset, length
		 *  4. update a segment information
		 *  5. delete the specific segment from the tree
		 *  6. get the current top node segment id
		 */

		private		BTree				m_tree;
		private		SegmentManager		m_sgManager;
		

		public SegTree(uint topSid, SegmentManager sgManager)
		{
			m_tree = new BTree(topSid, sgManager, new KSegId());
			m_sgManager = sgManager;
		}

#if DEBUG
		public BTree Tree
		{
			get { return m_tree;}
		}
#endif


		/// <summary>
		/// Given the segment id, return the offset, and the length of the segment related to the disk file.
		/// </summary>
		/// <param name="segId"></param>
		/// <param name="offset"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		public bool GetAddr(uint segId, ref uint offset, ref int length)
		{	
            KSegId target = new KSegId(segId);
			target = (KSegId)m_tree.Search(target);
			if (target != null && target.Addr != null)
			{
				offset = target.Addr.Offset;
				length = target.Addr.Length;
				return true;
			}
			else
				return false;
		}

		public bool InsertAddr(uint segId, uint offset, int length)
		{
			KSegId newKey = new KSegId(segId, new DSegAddr(offset, length));
			return m_tree.Insert(newKey);
		}

		public bool UpdateAddr(uint segId, uint offset, int length)
		{
			KSegId updatedKey = new KSegId(segId, new DSegAddr(offset, length));
			return m_tree.UpdateData(updatedKey);
		}

		public bool DeleteSegment(uint segId)
		{
			KSegId target = new KSegId(segId);
			return m_tree.Delete(target);
		}

		public uint TopNodSegId
		{
			get { return m_tree.TopNodSegId;}
		}

		public void Close()
		{
		}
	}
}
