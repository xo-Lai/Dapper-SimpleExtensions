using Dapper_SimpleExtensions;
using static Dapper_SimpleExtensions.Enums;

namespace Dapper_SimpleExtensions_Test
{
    public class DbConetxt : DapperDbContext
    {
        private static readonly string sqlconn = @"Data Source = .\sqlexpress;Initial Catalog=test;Integrated Security=True;MultipleActiveResultSets=true;";
        public DbConetxt(string connectionString, Dialect type) : base(connectionString, type)
        {

        }

        public static DbConetxt Get()
        {
            return new DbConetxt(sqlconn, Dialect.SQLServer);
        }

        public IDbSet<Student> Db_Student
        {
            get
            {
                return new DbSet<Student>(GetConnection(), Dialect.SQLServer);
            }
        }
    }
}
