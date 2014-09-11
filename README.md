OOD.NET
=======

An embedded Object Oriented Database (OOD) was built in C# in 2004 for my graduation project. 

A paper was published in an ACM Conference in 2005. It is available in the root folder of the repository ([direct link (pdf)](https://github.com/luanzhu/OOD.NET/blob/master/OOD-Conference-Paper.pdf?raw=true)).

Completed Features:
-------

1. Saving .NET objects directly to the disk based on primary key.
2. Searching by primary key at a fast speed.
3. Searching by fields other than primary key is possible at a slow speed.

Implementation Summary:
------

1. Used .NET reflection extensively to investigate class information and object states at run-time.
2. Designed a general B-tree with non-recursive operations, and used the B-tree to save class and object information to the database file, and to keep track of space usage of the database file.
3. Used 16KB memory usage bitmap to help track of page usages in the database and index files. 
4. Designed a general hash table with LRU (Least Recently Used page replacement) and used it to keep frequently used B-tree nodes in memory.


Usage
------
To save .NET objects to a database, you just need to add SchemaDefine attribute (along with the primary key designation) to its class definition.

For example:
```csharp
  [SchemaDefine(PrimaryKey="m_id")]
	public class Student
	{
		private			int				m_id;
		private			string			m_name;

		public Student(int id, string name)
		{
			m_id = id;
			m_name = name;
		}

		public int ID
		{
			get { return m_id;}
		}

		public string Name
		{
			get { return m_name;}
		}

	}
```

To save a new student record:
```csharp
    OOD.ObjectStore db = new Object.Store("students.ood");
 	
 	Student st = new Student(1, "Tom");
 	
 	//save the new student to database
 	db.Put(st); 
```

To search a student based on the ID filed:
```csharp
	String query = "select Demo.Student where m_id = 1";
	Student foundStudnet = (Student)o.SearchScalar(query);
```
 	

