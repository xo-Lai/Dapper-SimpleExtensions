using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Dapper_SimpleExtensions.Resolvers
{
    public class ColumnNameResolver : IColumnNameResolver
    {
        public virtual string ResolveColumnName(string columnEncapsulate, PropertyInfo propertyInfo)
        {
            var columnName = Common.Encapsulate(columnEncapsulate, propertyInfo.Name);

            var columnattr = propertyInfo.GetCustomAttributes(true).SingleOrDefault(attr => attr.GetType().Name == typeof(ColumnAttribute).Name) as dynamic;
            if (columnattr != null)
            {
                columnName = Common.Encapsulate(columnEncapsulate, columnattr.Name);
                if (Debugger.IsAttached)
                    Trace.WriteLine(String.Format("Column name for type overridden from {0} to {1}", propertyInfo.Name, columnName));
            }
            return columnName;
        }

    }
}
