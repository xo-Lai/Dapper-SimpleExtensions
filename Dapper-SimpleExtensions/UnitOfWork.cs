using System.Collections.Generic;
using System.Threading;

namespace Dapper_SimpleExtensions
{
    public class UnitOfWork
    {
        public static ThreadLocal<List<UnitStore>> ThreadLocal_Tag = new ThreadLocal<List<UnitStore>>();

        public static void Exec(DapperDbContext context)
        {
            context.Transaction(() =>
            {
                if (ThreadLocal_Tag.Value == null)
                {
                    return;
                }
                foreach (var item in ThreadLocal_Tag.Value)
                {
                    Dapper.SqlMapper.Execute(context.GetConnection(), item.Sql, item.Par, DapperDbContext.GetInnerTransaction());
                }
            });
        }



        public class UnitStore
        {
            public string Sql { get; set; }

            public string ConnStr { get; set; }

            public object Par { get; set; }

        }

        public static void AddToUnit<T>(T t, string sql)
        {
            if (UnitOfWork.ThreadLocal_Tag.Value == null)
            {
                UnitOfWork.ThreadLocal_Tag.Value = new List<UnitStore>();
            }
            UnitOfWork.ThreadLocal_Tag.Value.Add(new UnitOfWork.UnitStore()
            {
                Sql = sql,
                Par = t
            });
        }

    }
}
