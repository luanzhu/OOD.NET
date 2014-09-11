using OOD;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            InsertAndSearchStudents();
        }

        static void InsertAndSearchStudents()
        {
            const string dbFileName = "book5.ood";
            const string indexFileName = dbFileName + ".idx";
            const int limit = 600000;
            const int searchLimit = 1000;

            Console.WriteLine(string.Format(@"This program will try to create {0} People records and saved them in a database file, close the file, then re-open the file to query {1} records.", limit, searchLimit));
            Console.WriteLine("");

            if (File.Exists(dbFileName))
            {
                File.Delete(dbFileName);
            }
            if (File.Exists(indexFileName))
            {
                File.Delete(indexFileName);
            }

            OOD.ObjectStore os = new ObjectStore(dbFileName);

            Console.WriteLine("Database file was created. Ready to insert new records.");

            int succeed = 0, error = 0;
            System.DateTime start, end;

			for (int i=0; i<limit; i++)
			{
				Student st = new Student(
					i,
					"Yanhao Zhu" + i.ToString(),
					((i & 1) ==0 ? 'F' : 'M'),
					((i & 1) ==0 ? new DateTime(1976,1,1) : new DateTime(1974, 7, 27))
					);
				os.Put(st);

				if (i % 60 == 0)
				{
					Console.Write(i.ToString() + "\r");
				}
			}

            os.Close();
            Console.WriteLine("Database file was closed.");

            os = new ObjectStore(dbFileName);
            Console.WriteLine("Database file was re-opened. Ready to search.");

            succeed = error = 0;
            start = System.DateTime.Now;
            for (int i = 0; i < searchLimit; i++)
            {
                System.Random ran = new Random(i);
                int m = ran.Next(limit-1);

                string Query = "select Demo.Student where m_id = " + i.ToString();

                Student back = (Student)os.SearchScalar(Query);

                if (back != null)
                {
                    Console.Write(back.ID + "\r");
                    succeed++;
                }
                else
                    error++;
            }
            end = System.DateTime.Now;
            Console.WriteLine("OOD Search finished, {0} succeed, {1} failed. Takes {2}", succeed, error, end - start);

            os.Close();
            Console.Write("Database file was closed. Press any key to exit.");
            Console.Read();
        }
    }
}
