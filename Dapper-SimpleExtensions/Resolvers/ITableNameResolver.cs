using System;

namespace Dapper_SimpleExtensions.Resolvers
{
    public interface ITableNameResolver
    {
        string ResolveTableName(string tableEncapsulate, Type type);
    }
}
