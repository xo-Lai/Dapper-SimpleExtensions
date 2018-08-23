using MySql.Data.MySqlClient;
using Npgsql;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using static Dapper_SimpleExtensions.Enums;

namespace Dapper_SimpleExtensions
{
    public class DapperDbContext
    {

        private string _connectionString { get; set; }
        public static ThreadLocal<TransationWay?> ThreadLocal_Tag = new ThreadLocal<TransationWay?>();
        private static ThreadLocal<IDbConnection> ThreadLocal_Connection = new ThreadLocal<IDbConnection>();
        private static ThreadLocal<IDbTransaction> ThreadLocal_Transaction = new ThreadLocal<IDbTransaction>();
        private Dialect _dialect { get; set; }
        public DapperDbContext(string connectionString, Dialect type)
        {
            _connectionString = connectionString;
            _dialect = type;
        }

        private IDbConnection GetDbConnection()
        {
            IDbConnection conn = null;
            switch (_dialect)
            {
                case Dialect.SQLServer:
                    conn = new SqlConnection(_connectionString);
                    break;
                case Dialect.PostgreSQL:
                    conn = new NpgsqlConnection(_connectionString);
                    break;
                case Dialect.MySQL:
                    conn = new MySqlConnection(_connectionString);
                    break;
                default:
                    break;
            }
            return conn;
        }
        public IDbConnection GetConnection()
        {
            IDbConnection conn = null;
            if (ThreadLocal_Tag.Value == null)
            {
                conn = GetDbConnection();
                return conn;
            }

            if (ThreadLocal_Tag.Value == TransationWay.Transaction)
            {
                conn = (IDbConnection)ThreadLocal_Connection.Value;
                if (conn == null)
                {
                    conn = GetDbConnection();
                    BeginTransaction(conn);
                }

                return conn;
            }

            return conn;

        }

        #region  Transation
        public void Transaction(Action act)
        {
            if (ThreadLocal_Tag.Value != null)
            {
                throw new Exception("当前方法有未完成的事务,不能开启新的事务");
            }
            ThreadLocal_Tag.Value = TransationWay.Transaction;

            try
            {
                act();
            }
            catch (Exception e)
            {
                CompleteTransation(false);

                throw e;
            }

            CompleteTransation(true);

        }
        void CompleteTransation(bool isSucceed)
        {
            //clear 


            if (ThreadLocal_Connection.Value != null)
            {

                if (ThreadLocal_Transaction.Value == null)
                {
                    throw new Exception("Transaction is null");
                }
                if (isSucceed)
                {
                    ThreadLocal_Transaction.Value.Commit();
                }
                else
                {
                    ThreadLocal_Transaction.Value.Rollback();
                }

                var item = ThreadLocal_Connection.Value;

                if (item.State != ConnectionState.Closed)
                {
                    item.Close();
                    item.Dispose();
                }

            }
            ThreadLocal_Tag.Value = null;
            ThreadLocal_Transaction.Value = null;
            ThreadLocal_Connection.Value = null;
        }
        private void BeginTransaction(IDbConnection conn)
        {
            conn.Open();
            ThreadLocal_Connection.Value = conn;
            ThreadLocal_Transaction.Value = conn.BeginTransaction(IsolationLevel.ReadCommitted);
        }
        internal static IDbTransaction GetInnerTransaction()
        {
            if (ThreadLocal_Tag != null && ThreadLocal_Tag.Value == TransationWay.Transaction)
            {
                return ThreadLocal_Transaction.Value;
            }
            return null;
        }
        public IDbTransaction GetTransaction()
        {
            if (ThreadLocal_Tag != null && ThreadLocal_Tag.Value == TransationWay.Transaction)
            {
                return ThreadLocal_Transaction.Value;
            }
            return null;
        }

        #endregion


        #region UnitOfWork

        public void UnitWork(Action act)
        {
            ThreadLocal_Tag.Value = TransationWay.UnitOfWork;
            try
            {
                act();
                ThreadLocal_Tag.Value = null;
                //当这个时候，sql收集完毕，标记也变成了事务。
                UnitOfWork.Exec(this);
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                UnitOfWork.ThreadLocal_Tag.Value = null;
            }

        }
        #endregion
    }
}
