namespace Dapper_SimpleExtensions
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

        public static string BuildPage(this string sql, int pageNumber, int rowsPerPage)
        {
            return sql.Replace("{PageNumber}", pageNumber.ToString()).Replace("{RowsPerPage}", rowsPerPage.ToString()).Replace("{Offset}", ((pageNumber - 1) * rowsPerPage).ToString());
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
