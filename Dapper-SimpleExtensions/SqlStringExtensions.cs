﻿namespace Dapper_SimpleExtensions
{
    internal static class SqlStringExtensions
    {
        public static string BuildTableName(this string sql, string tableName)
        {
            return sql.Replace("{TableName}", tableName);
        }

        public static string BuildColumns(this string sql, string columns)
        {
            return sql.Replace("{SelectColumns}", columns);
        }

        public static string BuildPage(this string sql, int pageIndex, int pageSize)
        {
            return sql.Replace("{PageNumber}", pageIndex.ToString()).Replace("{RowsPerPage}", pageSize.ToString()).Replace("{Offset}", ((pageIndex - 1) * pageSize).ToString());
        }

        public static string BuildOrderBy(this string sql, string orderby)
        {
            return sql.Replace("{OrderBy}", orderby);
        }

        public static string BuildWhere(this string sql, string conditions)
        {
            return sql.Replace("{WhereClause}", conditions);
        }
    }
}
