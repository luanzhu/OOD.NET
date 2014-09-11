using System;
using System.Collections;
using System.Diagnostics;

namespace OOD.Imp.Storage
{
	/// <summary>
	/// Manage the segments in the system.
	/// </summary>
	public class dbSegmentManager : SegmentManager
	{

		private			uint				m_next_segment_id;
		private			CacheHashtable		m_cache;
		private			SegTree				m_segTree; //the tree used to manage the used segment space
		private			SpaceTree			m_spaceTree; // the tree used to manage the free segment in the tree
		private			int					m_cacheSize = 8179; //prime
		private			int					m_cacheLimit = 8192; //total node object cache in the memory
		private			DiskFile			m_diskFile = null;
		private			Segment				m_LRU; //for least recently used
		private			int					m_threshold = 256; //256 bytes


		public dbSegmentManager(uint nextSegmentId, SegTree segTree, SpaceTree spaceTree, DiskFile file)
		{
			m_next_segment_id = nextSegmentId;
			m_segTree = segTree;
			m_cache = new CacheHashtable(m_cacheSize);
			m_diskFile = file;
			m_spaceTree = spaceTree;
			m_LRU = new BNode();
		}

		public uint NextSegmentId
		{
			get { return m_next_segment_id;}
		}

		public override Segment GetSegment(uint segId, Segment segFactory, object helper)
		{
			Segment result = null;

			//check if this segment is already in cache
			if (m_cache.Exists(segId))
			{
				result = m_cache.Retrive(segId);

				//update the least recently used list
				result.BreakLinks();
				result.BuildLinks(m_LRU, m_LRU.Next);

				return result;
			}
			else
			{
				//if not, read that from disk
				uint offset = 0;
				int length = 0;
				if (m_segTree.GetAddr(segId, ref offset, ref length))
				{
					// put this segment into cache
                    byte[] content = m_diskFile.SynRead(offset, length);
					result = segFactory.Deserialize(segId, helper, content);

					_put_in_cache(result);
				}
				else
					throw new OOD.Exception.ProgramLogicError(
						this,
						"Trying to access a non-existing segment.");
				return result;	
			}
			
		}

		/// <summary>
		/// insert this segment into cache, kick out other if needed
		/// </summary>
		/// <param name="seg"></param>
		/// <precondition>seg is a new page brought in memory</precondition>
		/// <postcondition>seg is insert into the cache, and the front of the LRU list, kick out one page if needed.</postcondition>
		internal void _put_in_cache(Segment seg)
		{
			//if the cache is full, kick least recently used one out
			if (m_cache.Count > m_cacheLimit)
			{
				//pick least recently used one
				Segment victim = m_LRU.Prev;
				Debug.Assert(victim != m_LRU);
				if (victim.Dirty)
				{

					_write_back(victim);
				}
				victim.BreakLinks();
				bool t = m_cache.Delete(victim.SegmentID);//removed this one out of the cache
				Debug.Assert(t);
			}

			if (m_cache.Insert(seg) == false)
				throw new OOD.Exception.ProgramLogicError(
					this,
					"Trying to insert an existing key into CacheHashtable");
			seg.BuildLinks(m_LRU, m_LRU.Next); //move it the front of the LRU list
		}

		internal void _write_back(Segment seg)
		{
			//write it out to the disk
			byte[] content = seg.Serialize();

			//get its offset and length for the segment
			uint offset = 0;
			int length = -1;
			if (m_segTree.GetAddr(seg.SegmentID, ref offset, ref length))
			{

				Debug.Assert(length != -1);

				 if (content.Length > length)
				{
					//original space is too small

					//0. put original one fre
					m_spaceTree.SetSegmentFree(offset, length);

					//1. request a new segment with size = cotent.length + m_threshold;
					if (m_spaceTree.RequireSegment(content.Length+m_threshold, ref offset) == false)
						throw new OOD.Exception.ProgramLogicError(
							this,
							"Error happed while requesting free space.");

					//3. update the new addressing in m_segTree.
					if (m_segTree.UpdateAddr(seg.SegmentID, offset, content.Length + m_threshold) == false)
						throw new OOD.Exception.ProgramLogicError(
							this,
							"Erro happened while updating addressing.");

				}
				else if (content.Length < length - m_threshold)
				{

					//original space is too big, shrink it to length + 256;
					//1. update the addressing information in seqTree
//					int newLength = content.Length + m_threshold;
//					m_segTree.UpdateAddr(seg.SegmentID, offset, newLength);
//
//					//2. insert the left empty space into the spaceTree
//					uint leftOffset = (uint)(offset + newLength);
//					newLength = length - newLength;
//					m_spaceTree.SetSegmentFree(leftOffset, newLength);
				}
			}
			else
			{
				Debug.Assert(length == -1);

				//this is a new segment, it is never assigned adress before
				//1. Request a free space
				if (m_spaceTree.RequireSegment(content.Length + m_threshold, ref offset) == false)
					throw new OOD.Exception.ProgramLogicError(
						this,
						"Request space failed.");

				//2.put this into the m_segTree
				if (m_segTree.InsertAddr(seg.SegmentID, offset, content.Length + m_threshold)==false)
					throw new OOD.Exception.ProgramLogicError(
						this,
						"put segment addressing info failed in SegTree.");

			}

			m_diskFile.SynWrite(content, 0, offset, content.Length);

		}
		public override void GetNewSegment(Segment seg)
		{
			seg.SegmentID = m_next_segment_id;		
			seg.Initialize(m_next_segment_id++);
 
			_put_in_cache(seg);
		}

		public override void FreeSegment(Segment seg)
		{
			//remove it from cache
			this.m_cache.Delete(seg.SegmentID);
			//remove it from LRU list
			seg.BreakLinks();

			uint offset = 0;
			int length = -1;

			if (m_segTree.GetAddr(seg.SegmentID, ref offset, ref length))
			{
				Debug.Assert(length != -1);

				//this segment's addressing info is in the system.

				//put this segment into the free tree
				m_spaceTree.SetSegmentFree(offset, length);

				//remove its addressing information from segTree
				m_segTree.DeleteSegment(seg.SegmentID);
			}
			//if this segment is never written back to disk, just delete it from memory is enough
		}


		public void Flush()
		{
			foreach (Segment seg in m_cache)
			{
				if (seg.Dirty)
					_write_back(seg);
			}
		}

		public override void Close()
		{
			//write all the cache segement back to the disk
			Flush();

			//the memSegmentManger will be called from caller after this function
		}

	}
}
