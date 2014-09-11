using System;
using System.Diagnostics;

namespace OOD.Imp.Storage
{
	/// <summary>
	/// The hashtable used to keep all cached segment.
	/// </summary>
	public class CacheHashtable
	{
		private			int				m_size;
		private			CacheEntry[]	m_data;
		private			int				m_count;


		public int Count
		{
			get { return m_count;}
		}


		public CacheHashtable(int size)
		{
			m_size = size;
			m_data = new CacheEntry[size];
			m_count = 0;
		}

		public bool Insert(Segment seg)
		{
			Debug.Assert(Exists(seg.SegmentID) == false);

			int pos = (int)(seg.SegmentID % m_size);

			CacheEntry head = m_data[pos];

			if (head == null)
				m_data[pos] = new CacheEntry(seg);
			else
			{
				while (head.Next != null &&head.Segment.SegmentID != seg.SegmentID)
				{
					head = head.Next;
				}
				if (head.Segment.SegmentID == seg.SegmentID)
					return false;
				else
				{
					head.Next = new CacheEntry(seg,null);
				}
			}

			m_count ++;
			return true;
		}

		public bool Exists(uint segId)
		{
			int pos = (int)(segId % m_size);

			CacheEntry head = m_data[pos];

			if (head == null)
				return false;
			else
			{
				while (head != null)
				{
					if (head.Segment.SegmentID == segId)
						return true;
					head = head.Next;
				}
				return false;
			}
		}

		public bool Delete(uint segId)
		{
			int pos = (int)(segId % m_size);

			CacheEntry head = m_data[pos];

			if (head == null)
				return false;
			else
			{
				CacheEntry prev = null;
				while (head != null)
				{
					if (head.Segment.SegmentID == segId)
						break;
					prev = head;
					head = head.Next;
				}
				if (head != null) //found
				{
					if (prev == null)
						m_data[pos] = head.Next;
					else
						prev.Next = head.Next;
					m_count--;
					return true;
				}
				else
					return false;
			}
		}

		public Segment Retrive(uint segId)
		{
			int pos = (int)(segId % m_size);

			CacheEntry head = m_data[pos];

			if (head == null)
				return null;
			else
			{
				while (head != null)
				{
					if (head.Segment.SegmentID == segId)
						break;
					head = head.Next;
				}
				if (head != null) //found
				{
					return head.Segment;
				}
				else
					return null;
			}
		}


		public MyEnumerator GetEnumerator() 
		{
			return new MyEnumerator(this);
		}

		// Declare the enumerator class:
		public class MyEnumerator 
		{
			CacheEntry m_current = null;
			CacheHashtable m_table;
			int m_row = 0;
			public MyEnumerator(CacheHashtable table) 
			{
				m_table = table;
			}

			public bool MoveNext() 
			{
				if (m_current == null)
				{
					while (m_row < m_table.m_size && m_table.m_data[m_row] == null)
					{
						m_row ++;
					}
					if (m_row < m_table.m_size)
						m_current = m_table.m_data[m_row];
					else
						m_current = null;
					m_row ++;
				}
				else
				{
					if (m_current.Next != null)
						m_current = m_current.Next;
					else
					{
						while (m_row < m_table.m_size && m_table.m_data[m_row] == null)
						{
							m_row ++;
						}
						if (m_row < m_table.m_size)
							m_current = m_table.m_data[m_row];
						else
							m_current = null;
						m_row ++;
					}
				}

				return m_current != null;
			}

			public Segment Current 
			{
				get 
				{
					return m_current.Segment;
				}
			}

		}
	}
}
