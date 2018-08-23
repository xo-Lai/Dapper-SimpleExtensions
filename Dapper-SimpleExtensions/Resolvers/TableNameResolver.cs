using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Linq;


namespace Dapper_SimpleExtensions.Resolvers
{
    public class TableNameResolver : ITableNameResolver
    {

        public virtual string ResolveTableName(string tableEncapsulate, Type type)
        {
            var tableName = Common.Encapsulate(tableEncapsulate, type.Name);

            var tableattr = type.GetCustomAttributes(true).SingleOrDefault(attr => attr.GetType().Name == typeof(TableAttribute).Name) as dynamic;
            if (tableattr != null)
            {
                tableName = Common.Encapsulate(tableEncapsulate, tableattr.Name);
                try
                {
                    if (!String.IsNullOrEmpty(tableattr.Schema))
                    {
                        string schemaName = Common.Encapsulate(tableEncapsulate, tableattr.Schema);
                        tableName = String.Format("{0}.{1}", schemaName, tableName);
                    }
                }
                catch (RuntimeBinderException)
                {
                    //Schema doesn't exist on this attribute.
                }
            }

            return tableName;
        }

    }
}
