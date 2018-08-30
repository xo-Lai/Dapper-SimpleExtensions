using System;
using Dapper;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper_SimpleExtensions
{
    /// <summary>
    /// 异步查询
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public partial class DbQuery<TResult>
    {

        public Task<IEnumerable<TResult>> GetListAsync()
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

            return _connection.QueryAsync<TResult>(query, whereAndargs.Item2, DapperDbContext.GetInnerTransaction());
        }

        public async Task<PageData<TResult>> GetListPagedAsync(int pageNumber, int rowPerpage)
        {
            if (string.IsNullOrEmpty(_getPagedListSql))
                throw new Exception("GetListPage is not supported with the current SQL Dialect");

            if (pageNumber < 1)
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
                .BuildPage(pageNumber, rowPerpage)
                .BuildOrderBy(SqlManipulationExtensions.BuildOrderBy(_orderbyExp, _orderbyAseOrDesc));
            var total = await RecordCountAsync();
            var list = await _connection.QueryAsync<TResult>(query, whereAndargs.Item2, DapperDbContext.GetInnerTransaction());
            return new PageData<TResult>(list, total, pageNumber, rowPerpage);
        }

        public Task<TResult> GetFisrtAsync()
        {
            var currenttype = typeof(TResult);
            var tableName = SqlManipulationExtensions.GetTableName(currenttype);
            var whereAndargs = SqlManipulationExtensions.BuildWhere(_whereExp);
            var query = _getListSql;
            query = query.BuildTableName(tableName)
                .BuildColumns(SqlManipulationExtensions.BuildSelect<TResult>())
                .BuildWhere(whereAndargs.Item1)
                .BuildOrderBy(SqlManipulationExtensions.BuildOrderBy(_orderbyExp, _orderbyAseOrDesc));
            return _connection.QueryFirstOrDefaultAsync<TResult>(query, whereAndargs.Item2, DapperDbContext.GetInnerTransaction());
        }

        public Task<long> RecordCountAsync()
        {
            var currenttype = typeof(TResult);
            var idProps = SqlManipulationExtensions.GetIdProperties(currenttype).ToList();
            if (!idProps.Any())
                throw new ArgumentException("Entity must have at least one [Key] property");

            var tableName = SqlManipulationExtensions.GetTableName(currenttype);
            var whereAndargs = SqlManipulationExtensions.BuildWhere(_whereExp);
            string query = $" Select count(1) from {tableName} {whereAndargs.Item1}";
            return _connection.ExecuteScalarAsync<long>(query, whereAndargs.Item2, DapperDbContext.GetInnerTransaction());
        }

        public Task<TResult> GetAsync(object id)
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
            return _connection.QueryFirstOrDefaultAsync<TResult>(query, whereAndargs.Item2, DapperDbContext.GetInnerTransaction());
        }
    }
}
