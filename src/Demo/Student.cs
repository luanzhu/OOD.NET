using System;
using OOD;

namespace Demo
{
	[SchemaDefine(PrimaryKey="m_id")]
	public class Student
	{
		private			int				m_id;
		private			string			m_name;
		private			char			m_sex;
		private			DateTime		m_birthDate;
		private			int				m_age;


		public Student(int id, string name, char sex, DateTime birthDate)
		{
			m_id = id;
			m_name = name;
			m_sex = sex;
			m_birthDate = birthDate;
			
			m_age = System.DateTime.Now.Year - birthDate.Year;
		}

		public Student()
		{
		}

		public int ID
		{
			get { return m_id;}
		}

		public string Name
		{
			get { return m_name;}
		}

		public char Sex
		{
			get { return m_sex;}
		}

		public DateTime BirthDate
		{
			get { return m_birthDate;}
		}

		public int Age
		{
			get { return m_age;}
		}
	}
}
