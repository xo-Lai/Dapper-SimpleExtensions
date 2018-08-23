using Dapper_SimpleExtensions;
using System;


namespace Dapper_SimpleExtensions_Test
{
    public class Student
    {
        [Key]
        public int Sno { get; set; }
        public string Sname { get; set; }
        public string Ssex { get; set; }
        public int Sage { get; set; }
        public string Sdept { get; set; }
        public DateTime Stime { get; set; }
        public TimeSpan Stime2 { get; set; }
    }
}
