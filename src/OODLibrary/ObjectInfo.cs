using System;

namespace OOD
{
	/// <summary>
	/// class which will store the information needed for object serialize back.
	/// </summary>
	public class ObjectInfo
	{
		private				object					m_primary_key_value;
		private				Types.InternalType			m_primary_key_type;
		private				string[]				m_other_fields;

		public ObjectInfo(Types.InternalType primaryKeyType, object primaryKeyValue, string[] otherFields)
		{
			m_primary_key_value = primaryKeyValue;
			m_primary_key_type = primaryKeyType;
			m_other_fields = otherFields;
		}

		public object PrimaryKeyValue
		{
			get { return m_primary_key_value;}
			set { m_primary_key_value = value;}
		}

		public Types.InternalType PrimaryKeyType
		{
			get { return m_primary_key_type;}
			set { m_primary_key_type = value;}
		}

		public string[] SerializedOtherFieldValues
		{
			get { return m_other_fields;}
			set { m_other_fields = value;}
		}
	}
}
