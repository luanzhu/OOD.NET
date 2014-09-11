using System;
using System.Diagnostics;
using System.Collections;

namespace OOD.Imp.Storage
{
	/*   File format in the helper file:
	 *   [0-512] file header
	 *   [512 -- 16K + 512] page usage bitmap
	 *   [512 bytes][512 bytes]...... all other are b-tree pages with size of 256 bytes. 
	 * 
	 */
	/// <summary>
	/// will be used to store/monitor page usage for the b-tree used in the dbSegmentManager.
	/// will also be used to store the free space information in the database file.
	/// </summary>
	public class memSegmentManager : SegmentManager
	{

		private			PageBitmap		m_bitmap;
		private			DiskFile		m_diskFile;
		private			int				m_pageBits = 9; //1<<9, 512 bytes
		private			int				m_pageSize;
		private			int				m_cacheSize = 1031; //1031 is a prime
		private			CacheHashtable	m_cache;  
		private			int				m_cacheLimit = 2048; //cache size 1048 x 512 = 1MB, bigger than this size, in fact
		private			Segment			m_LRU; //for least recently used
		private			FileHeader		m_fileHeader; //the only file header in the two files, it is located at page 0.
		
		public memSegmentManager(DiskFile file)
		{
			m_cache = new CacheHashtable(m_cacheSize);

			m_pageSize = 1 << m_pageBits;
			m_diskFile = file;
			m_bitmap = new PageBitmap();
			m_fileHeader = new FileHeader();
			if (file.Length < m_bitmap.Data.Length + m_pageSize*2) //page 0 for fileheader, page 33 for segTree, page 44 for spacetree
			{
				//this helper file is a new one

				for (int i=0; i<(m_bitmap.Data.Length >> (m_pageBits + 3)); i++)
					m_bitmap.Data[i] = 0xFF;
				m_bitmap.SetPageTaken(m_bitmap.GetFirstFreePageID()); //one more page taken because of the fileheader
				m_bitmap.Dirty = true;

				m_fileHeader.SetInitializeNeeded();
			}
			else 
			{
				byte[] header = file.SynRead(0, m_pageSize);
				m_fileHeader.Deserialize(header);
				file.SynRead(m_bitmap.Data, 0, m_pageSize, m_bitmap.Data.Length);
			}

			m_LRU = new BNode();
		}

		public FileHeader Header
		{
			get { return m_fileHeader;}
		}

		public override Segment GetSegment(uint segId, Segment segFactory, object helper)
		{
			Debug.Assert(segId >= (m_bitmap.Data.Length >> m_pageBits));
			Debug.Assert(m_bitmap.PageFree(segId) == false);

			if (m_cache.Exists(segId))
			{
				//requested page is in the cache
				Segment seg = m_cache.Retrive(segId);
				//update recently used page list, move this one to the front of the list, most recently used.
				seg.BreakLinks();
				seg.BuildLinks(m_LRU, m_LRU.Next);

				return seg;
			}
			else
			{
				//the requested page is not in the cache, read it from disk
				byte[] content = m_diskFile.SynRead(segId*m_pageSize, m_pageSize);
				Segment seg = segFactory.Deserialize(segId, helper, content);

				_put_in_cache(seg);
				
				return seg;
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
					//write it out to the disk
					byte[] content = victim.Serialize();
					Debug.Assert(content.Length <= m_pageSize);
					m_diskFile.SynWrite(content, 0, victim.SegmentID*m_pageSize, content.Length);
				}
				victim.BreakLinks();
				bool t = m_cache.Delete(victim.SegmentID);//removed this one out of the cache
				Debug.Assert(t);
			}

			if (m_cache.Insert(seg) == false)
				throw new OOD.Exception.ProgramLogicError(
					this,
					"Trying to insert a existing key into CacheHashtable.");
			seg.BuildLinks(m_LRU, m_LRU.Next); //move it the front of the LRU list
		}
		public override void GetNewSegment(Segment seg)
		{
			uint segId = m_bitmap.GetFirstFreePageID();
			seg.Initialize(segId);
			m_bitmap.SetPageTaken(segId);

			//put the new node into cache
			//kick out one if needed

			_put_in_cache(seg);
		}

		public override void FreeSegment(Segment seg)
		{
			m_bitmap.SetPageFree(seg.SegmentID);

			//remove that one from cache
			m_cache.Delete(seg.SegmentID);

			//remove it from the LRU list also
			seg.BreakLinks();
		}

		public void Flush()
		{
			if (m_fileHeader.Dirty)
			{
				byte[] header = m_fileHeader.Serialize();
				Debug.Assert(header.Length <= m_pageSize);
				m_diskFile.SynWrite(header, 0, 0, header.Length);
			}
			if (m_bitmap.Dirty)
			{
				m_diskFile.SynWrite(m_bitmap.Data, 0, m_pageSize, m_bitmap.Data.Length);
			}
			foreach (Segment seg in m_cache)
			{
				if (seg.Dirty)
				{
					byte[] content = seg.Serialize();
					Debug.Assert(content.Length <= m_pageSize);
					m_diskFile.SynWrite(content, 0, seg.SegmentID*m_pageSize, content.Length);
				}
			}
		}

		public void SetNormalClosed()
		{
			m_diskFile.SynWriteByte(1,0);
		}

		public override void Close()
		{
			Flush();
		}


	}
}
