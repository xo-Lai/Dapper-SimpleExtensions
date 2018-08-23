using System;
using System.Collections.Generic;

namespace Dapper_SimpleExtensions
{
    internal static class Common
    {
        public static string Encapsulate(string encapsulate, string databaseword)
        {
            return string.Format(encapsulate, databaseword);
        }
        public static bool IsSimpleType(this Type type)
        {
            var underlyingType = Nullable.GetUnderlyingType(type);
            type = underlyingType ?? type;
            var simpleTypes = new List<Type>
                               {
                                   typeof(byte),
                                   typeof(sbyte),
                                   typeof(short),
                                   typeof(ushort),
                                   typeof(int),
                                   typeof(uint),
                                   typeof(long),
                                   typeof(ulong),
                                   typeof(float),
                                   typeof(double),
                                   typeof(decimal),
                                   typeof(bool),
                                   typeof(string),
                                   typeof(char),
                                   typeof(Guid),
                                   typeof(DateTime),
                                   typeof(TimeSpan),
                                   typeof(DateTimeOffset),
                                   typeof(byte[])
                               };
            return simpleTypes.Contains(type) || type.IsEnum;
        }
    }

    public class Enums
    {

        public enum TransationWay
        {
            Transaction,
            UnitOfWork
        }
        public enum Dialect
        {
            SQLServer,
            PostgreSQL,
            MySQL,
        }
    }
}
