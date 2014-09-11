using System;
using System.Diagnostics;

namespace OOD.Imp.Storage
{
	/// <summary>
	/// The class of the items stored in the catalog tree cache for found (className-->classInfo), (cid-->classInfo)
	/// </summary>
	public class ClassEntry : OOD.ClassInfo
	{
		private				ClassEntry			m_prev;
		private				ClassEntry			m_next;

		public ClassEntry(uint cid, string className, string[] fields, uint topNodeSId)
			:base(cid, className, fields, topNodeSId)
		{			
			m_prev = m_next = this;
		}

		public ClassEntry()
		{
			m_prev = m_next = this;
		}

		public ClassEntry Previous
		{
			get { return m_prev;}
		}

		public ClassEntry Next
		{
			get { return m_next;}
		}

		public void BreakLinks()
		{
			this.m_prev.m_next = this.m_next;
			this.m_next.m_prev = this.m_prev;

			this.m_prev = this.m_next = this;
		}

		public void BuildLinks(ClassEntry prev, ClassEntry next)
		{
			this.m_next = next;
			this.m_prev = prev;

			prev.m_next = this;
			next.m_prev = this;
		}

	}
}
