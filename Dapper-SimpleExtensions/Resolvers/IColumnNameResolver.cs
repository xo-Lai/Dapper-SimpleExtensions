using System.Reflection;

namespace Dapper_SimpleExtensions.Resolvers
{
    public interface IColumnNameResolver
    {
        string ResolveColumnName(string columnEncapsulate, PropertyInfo propertyInfo);
    }
}
