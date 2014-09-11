using System;

namespace OOD.Exception
{
	/// <summary>
	/// Custom exception class for OOD related run-time checking.
	/// </summary>
	public class OODException : System.Exception
	{
		public OODException(object sender, string message)
		{
			this.m_Sender = sender;
			this.m_Message = message;
		}

		public object Sender
		{
			get { return this.m_Sender;}
		}

		public override string Message
		{
			get { return this.m_Message;}
		}

		private object m_Sender;
		private string m_Message;

	}

	public class ArrayIndexOutofRangeException : OODException
	{
		public ArrayIndexOutofRangeException(object sender, string message)
		:base(sender, message)
		{

		}
	}

	public class PageIndexOutofRangeException : OODException
	{
		public PageIndexOutofRangeException(object sender, string message)
			:base(sender, message)
		{
		}
	}

	public class ProgramLogicError : OODException
	{
		public ProgramLogicError(object sender, string message) 
			: base(sender, message)
		{
		}
	}

	public class OutofSpaceError : OODException
	{
		public OutofSpaceError(object sender, string message)
			: base(sender, message)
		{

		}
	}

	public class NotImplemented : OODException
	{
		public NotImplemented(object sender, string message)
			: base(sender, message)
		{
		}
	}

	public class MissingPrimaryKey : OODException
	{
		public MissingPrimaryKey(object sender, string message)
			: base(sender, message)
		{
		}
	}

	public class DuplicatedPrimaryKey : OODException
	{
		public DuplicatedPrimaryKey(object sender, string message)
			: base(sender, message)
		{
		}
	}

	public class SchemeDefinition : OODException
	{
		public SchemeDefinition(object sender, string message)
			: base(sender, message)
		{
		}
	}

}
