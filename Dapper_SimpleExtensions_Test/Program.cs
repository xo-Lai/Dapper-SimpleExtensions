using System;
using System.Linq;
using static Dapper_SimpleExtensions.Enums;

namespace Dapper_SimpleExtensions_Test
{
    class Program
    {
     
        static void Main(string[] args)
        {
            var list = DbConetxt.Get().Db_Student.Where(s=>s.Sage==10).GetListPaged(1, 3);
            System.Console.WriteLine(list.Total);
    
            System.Console.ReadKey();
        }
    }
}
