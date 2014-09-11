using System;

namespace OOD.Imp.Storage
{
	/// <summary>
	/// The base for all the segments. Currently only b-tree node.
	/// </summary>
	public abstract class Segment
	{

		protected			uint				m_segment_id;
		protected			bool				m_dirty = false;

		//recently used list
		protected			Segment				m_prev;
		protected			Segment				m_next;

		protected Segment()
		{
			m_prev = m_next = this;
		}

		public uint SegmentID
		{
			get { return m_segment_id;}
			set 
			{
				m_segment_id = value;
				this.m_dirty = true;
			}
		}

		public bool Dirty
		{
			get { return m_dirty;}
			set { m_dirty = value;}
		}

		public Segment Prev
		{
			get { return m_prev;}
			set { m_prev = value;}
		}

		public Segment Next
		{
			get { return m_next;}
			set { m_next = value;}
		}

		public void BuildLinks(Segment prev, Segment next)
		{
			m_prev = prev;
			m_next = next;

			prev.m_next = this;
			next.m_prev = this;
		}

		public void BreakLinks()
		{
			m_prev.m_next = m_next;
			m_next.m_prev = m_prev;

			m_next = m_prev = this;
		}

		public abstract void Initialize(uint segmentId);
		public abstract byte[] Serialize();
		public abstract Segment Deserialize(uint segmentId, object helper, byte[] bytes);
		public abstract Segment Deserialize(uint segmentId, object helper, byte[] bytes, int offset, int count);
	}
}
