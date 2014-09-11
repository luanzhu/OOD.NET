using System;

namespace OOD
{
	/// <summary>
	/// The entry in LRU hashtable.
	/// </summary>
	public class LRUHashEntry
	{
		private			object				m_data;
		private			object				m_key;
		private			LRUHashEntry		m_next; //the link list for collision control

		private			LRUHashEntry		m_LRU_prev; //deal with LRU list
		private			LRUHashEntry		m_LRU_next; 


		public LRUHashEntry(object key, object data, LRUHashEntry next)
		{
			m_key = key;
			m_data = data;
			m_next = next;
			m_LRU_prev = m_LRU_next = this;
		}

		public LRUHashEntry(object key, object data)
		{
			m_key = key;
			m_data = data;
			m_next = null;
			m_LRU_prev = m_LRU_next = this;
		}

		public LRUHashEntry()
		{
			m_key = null;
			m_data = null;
			m_next = null;
			m_LRU_prev = m_LRU_next = this;
		}

		public object Key
		{
			get { return m_key;}
		}

		public object Data 
		{
			get { return m_data;}
			set { m_data = value;}
		}
		public LRUHashEntry Next
		{
			get { return m_next;}
			set { m_next = value;}
		}

		public LRUHashEntry LRU_Prev
		{
			get { return m_LRU_prev;}
			set { m_LRU_prev = value;}
		}

		public LRUHashEntry LRU_Next
		{
			get { return m_LRU_next;}
			set { m_LRU_next = value;}
		}

		public void BreakLinks()
		{
			m_LRU_prev.m_LRU_next = m_LRU_next;
			m_LRU_next.m_LRU_prev = m_LRU_prev;

			m_LRU_prev = m_LRU_next = this;
		}

		public void BuildLinks(LRUHashEntry LRU_prev, LRUHashEntry LRU_next)
		{
			LRU_prev.m_LRU_next = this;
			LRU_next.m_LRU_prev = this;

			m_LRU_prev = LRU_prev;
			m_LRU_next = LRU_next;
		}
	}
}
