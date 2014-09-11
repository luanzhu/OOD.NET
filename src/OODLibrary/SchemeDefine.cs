using System;

namespace OOD
{
	/// <summary>
	/// Each persistable class has to declare this attribute for the primary key.
	/// </summary>
	public class SchemaDefine : System.Attribute
	{
		public SchemaDefine()
		{
		}

		private string m_primaryKey;
		public string PrimaryKey
		{
			get { return this.m_primaryKey;}
			set { this.m_primaryKey = value;}
		}

	}
}
