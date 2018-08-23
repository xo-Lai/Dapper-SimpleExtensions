using System;

namespace Dapper_SimpleExtensions_Test
{
    class Program
    {
        static void Main(string[] args)
        {
            //var studentId = DbConetxt.Get().Db_Student.Insert(new Student { Sname = "www", Sage = 23, Stime = DateTime.Now });
            //System.Console.WriteLine(studentId);
            var list = DbConetxt.Get().Db_Student.GetList();
            System.Console.ReadKey();
        }
    }
}
