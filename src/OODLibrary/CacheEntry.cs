using System;

namespace OOD.Imp.Storage
{
	/// <summary>
	/// The entry in the hash table. Will be used in the cache table for segment in memory
	/// </summary>
	public class CacheEntry
	{
		private			Segment			m_segment;
		private			CacheEntry		m_next;


		public CacheEntry(Segment seg)
		{
			m_segment = seg;
			m_next = null;
		}

		public CacheEntry(Segment seg, CacheEntry next)
		{
			m_segment = seg;
			m_next = next;
		}

		public CacheEntry()
		{
			m_segment = null;
			m_next = null;
		}

		public Segment Segment
		{
			get { return m_segment;}
			set { m_segment = value;}
		}
		public CacheEntry Next
		{
			get { return m_next;}
			set { m_next = value;}
		}
	}
}
