using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using static Dapper_SimpleExtensions.Enums;

namespace Dapper_SimpleExtensions
{
    public class DbQuery<TResult> : IQuery<TResult> where TResult : class
    {
        public DbQuery(IDbConnection connection, Dialect dialect)
        {
            _connection = connection;
            _dialect = dialect;
            SetDialect();
        }
        protected IDbConnection _connection;
        private Dialect _dialect { set; get; }
        public string _getIdentitySql;
        private string _getPagedListSql;
        private string _getListSql = "Select {SelectColumns} from {TableName} {WhereClause} Order By {OrderBy}";


        private string _orderbyAseOrDesc = "";
        private Expression<Func<TResult, bool>> _whereExp { get; set; }
        private Expression<Func<TResult, object>> _orderbyExp { get; set; }
        private DynamicParameters args = new DynamicParameters();



        /// <summary>
        /// Sets the database dialect 
        /// </summary>
        /// <param name="dialect"></param>
        private void SetDialect()
        {
            switch (_dialect)
            {
                case Dialect.PostgreSQL:

                    SqlManipulationExtensions.SetEncaapsulate("\"{0}\"");
                    _getIdentitySql = string.Format("SELECT LASTVAL() AS id");
                    _getPagedListSql = "Select {SelectColumns} from {TableName} {WhereClause} Order By {OrderBy} LIMIT {RowsPerPage} OFFSET (({PageNumber}-1) * {RowsPerPage})";
                    break;

                case Dialect.MySQL:

                    SqlManipulationExtensions.SetEncaapsulate("`{0}`");
                    _getIdentitySql = string.Format("SELECT LAST_INSERT_ID() AS id");
                    _getPagedListSql = "Select {SelectColumns} from {TableName} {WhereClause} Order By {OrderBy} LIMIT {Offset},{RowsPerPage}";
                    break;
                default:

                    SqlManipulationExtensions.SetEncaapsulate("[{0}]");
                    _getIdentitySql = string.Format("SELECT CAST(SCOPE_IDENTITY()  AS BIGINT) AS [id]");
                    _getPagedListSql = "SELECT * FROM (SELECT ROW_NUMBER() OVER(ORDER BY {OrderBy}) AS PagedNumber, {SelectColumns} FROM {TableName} {WhereClause}) AS u WHERE PagedNUMBER BETWEEN (({PageNumber}-1) * {RowsPerPage} + 1) AND ({PageNumber} * {RowsPerPage})";
                    break;
            }
        }

        /// <summary>
        /// Used before GetList or GetPaged  Or GetFirst
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        public IQuery<TResult> Where(Expression<Func<TResult, bool>> exp)
        {
            _whereExp = exp;
            return this;
        }

        /// <summary>
        /// order by aes 
        /// Used before GetList or GetPaged  Or GetFirst
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        public IQuery<TResult> OrderBy(Expression<Func<TResult, object>> exp)
        {
            _orderbyExp = exp;
            _orderbyAseOrDesc = "  ASC ";

            return this;
        }

        /// <summary>
        /// order by desc
        /// Used before GetList or GetPaged  Or GetFirst
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        public IQuery<TResult> OrderByDesc(Expression<Func<TResult, object>> exp)
        {
            _orderbyExp = exp;
            _orderbyAseOrDesc = "  DESC ";
            return this;
        }


        public IEnumerable<TResult> GetList()
        {
            var currenttype = typeof(TResult);
            var idProps = SqlManipulationExtensions.GetIdProperties(currenttype).ToList();
            if (!idProps.Any())
                throw new ArgumentException("Entity must have at least one [Key] property");

            var tableName = SqlManipulationExtensions.GetTableName(currenttype);
            var whereAndargs = SqlManipulationExtensions.BuildWhere(_whereExp);
            var query = _getListSql;
            query = query.BuildTableName(tableName)
                .BuildColumns(SqlManipulationExtensions.BuildSelect<TResult>())
                .BuildWhere(whereAndargs.Item1)
                .BuildOrderBy(SqlManipulationExtensions.BuildOrderBy(_orderbyExp, _orderbyAseOrDesc));

            return _connection.Query<TResult>(query, whereAndargs.Item2, DapperDbContext.GetInnerTransaction());
        }


        public IEnumerable<TResult> GetListPaged(int pageIndex, int pageSize)
        {

            if (string.IsNullOrEmpty(_getPagedListSql))
                throw new Exception("GetListPage is not supported with the current SQL Dialect");

            if (pageIndex < 1)
                throw new Exception("Page must be greater than 0");
            var currenttype = typeof(TResult);
            var idProps = SqlManipulationExtensions.GetIdProperties(currenttype).ToList();
            if (!idProps.Any())
                throw new ArgumentException("Entity must have at least one [Key] property");

            var query = _getPagedListSql;
            var tableName = SqlManipulationExtensions.GetTableName(currenttype);
            var whereAndargs = SqlManipulationExtensions.BuildWhere(_whereExp);
            query = query.BuildTableName(tableName)
                .BuildColumns(SqlManipulationExtensions.BuildSelect<TResult>())
                .BuildWhere(whereAndargs.Item1)
                .BuildPage(pageIndex, pageSize)
                .BuildOrderBy(SqlManipulationExtensions.BuildOrderBy(_orderbyExp, _orderbyAseOrDesc));

            return _connection.Query<TResult>(query, whereAndargs.Item2, DapperDbContext.GetInnerTransaction());

        }

        public TResult GetFisrt()
        {
            var currenttype = typeof(TResult);
            var tableName = SqlManipulationExtensions.GetTableName(currenttype);
            var whereAndargs = SqlManipulationExtensions.BuildWhere(_whereExp);
            var query = _getListSql;
            query = query.BuildTableName(tableName)
                .BuildColumns(SqlManipulationExtensions.BuildSelect<TResult>())
                .BuildWhere(whereAndargs.Item1)
                .BuildOrderBy(SqlManipulationExtensions.BuildOrderBy(_orderbyExp, _orderbyAseOrDesc));
            return _connection.QueryFirstOrDefault<TResult>(query, whereAndargs.Item2, DapperDbContext.GetInnerTransaction());
        }

        public long RecordCount()
        {
            var currenttype = typeof(TResult);
            var idProps = SqlManipulationExtensions.GetIdProperties(currenttype).ToList();
            if (!idProps.Any())
                throw new ArgumentException("Entity must have at least one [Key] property");

            var tableName = SqlManipulationExtensions.GetTableName(currenttype);
            var whereAndargs = SqlManipulationExtensions.BuildWhere(_whereExp);
            string query = $" Select count(1) from {tableName} {whereAndargs.Item1}";
            return _connection.ExecuteScalar<long>(query, whereAndargs.Item2, DapperDbContext.GetInnerTransaction());
        }

        public bool Exist()
        {
            return RecordCount() > 0;
        }

        public TResult Get(object id)
        {
            var currenttype = typeof(TResult);
            var idProps = SqlManipulationExtensions.GetIdProperties(currenttype).ToList();
            if (!idProps.Any())
                throw new ArgumentException("Entity must have at least one [Key] property");


            string where = $" where {SqlManipulationExtensions.GetColumnName(idProps.First())} = '{id}'";
            var tableName = SqlManipulationExtensions.GetTableName(currenttype);
            var whereAndargs = SqlManipulationExtensions.BuildWhere(_whereExp, where);
            var query = _getListSql;
            query = query.BuildTableName(tableName)
                .BuildColumns(SqlManipulationExtensions.BuildSelect<TResult>())
                .BuildWhere(whereAndargs.Item1)
                .BuildOrderBy(SqlManipulationExtensions.BuildOrderBy(_orderbyExp, _orderbyAseOrDesc));
            return _connection.QueryFirstOrDefault<TResult>(query, whereAndargs.Item2, DapperDbContext.GetInnerTransaction());
        }
    }
}
