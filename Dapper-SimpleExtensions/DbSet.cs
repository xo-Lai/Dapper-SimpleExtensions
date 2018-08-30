using Dapper;
using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper_SimpleExtensions
{
    public class DbSet<TEntity> : DbQuery<TEntity>, IDbSet<TEntity> where TEntity : class
    {
        public DbSet(IDbConnection connection, Enums.Dialect dialect) : base(connection, dialect)
        {
        }

        public int Delete(object id)
        {
            var currenttype = typeof(TEntity);
            var idProps = SqlManipulationExtensions.GetIdProperties(currenttype).ToList();
            if (!idProps.Any())
                throw new ArgumentException("Delete<T> only supports an entity with a [Key] or Id property");
            var name = SqlManipulationExtensions.GetTableName(currenttype);
            string where = SqlManipulationExtensions.BuildWhere<TEntity>(idProps);
            string deleteSqlStr = $"delete from {name} where {where}";

            var dynParms = new DynamicParameters();
            dynParms.Add("@" + idProps.First().Name, id);
            if (DapperDbContext.ThreadLocal_Tag.Value == Enums.TransationWay.UnitOfWork)
            {
                UnitOfWork.AddToUnit(dynParms, deleteSqlStr);
                return 0;
            }
            return _connection.Execute(deleteSqlStr, dynParms, DapperDbContext.GetInnerTransaction());

        }

        public int Delete(TEntity entityToDelete)
        {
            var currenttype = typeof(TEntity);
            var idProps = SqlManipulationExtensions.GetIdProperties(currenttype).ToList();
            if (!idProps.Any())
                throw new ArgumentException("Entity must have at least one [Key] or Id property");

            var name = SqlManipulationExtensions.GetTableName(currenttype);
            var where = SqlManipulationExtensions.BuildWhere<TEntity>(idProps, entityToDelete);
            string deleteSqlStr = $"delete from {name} where {where}";
            if (DapperDbContext.ThreadLocal_Tag.Value == Enums.TransationWay.UnitOfWork)
            {
                UnitOfWork.AddToUnit(entityToDelete, deleteSqlStr);
                return 0;
            }
            return _connection.Execute(deleteSqlStr, entityToDelete, DapperDbContext.GetInnerTransaction());
        }

        public Task<int> DeleteAsync(object id)
        {
            var currenttype = typeof(TEntity);
            var idProps = SqlManipulationExtensions.GetIdProperties(currenttype).ToList();
            if (!idProps.Any())
                throw new ArgumentException("Delete<T> only supports an entity with a [Key] or Id property");
            var name = SqlManipulationExtensions.GetTableName(currenttype);
            string where = SqlManipulationExtensions.BuildWhere<TEntity>(idProps);
            string deleteSqlStr = $"delete from {name} where {where}";

            var dynParms = new DynamicParameters();
            dynParms.Add("@" + idProps.First().Name, id);
            if (DapperDbContext.ThreadLocal_Tag.Value == Enums.TransationWay.UnitOfWork)
            {
                UnitOfWork.AddToUnit(dynParms, deleteSqlStr);
                return new Task<int>(() => 0); 
            }
            return _connection.ExecuteAsync(deleteSqlStr, dynParms, DapperDbContext.GetInnerTransaction());

        }

        public Task<int> DeleteAsync(TEntity entityToDelete)
        {
            var currenttype = typeof(TEntity);
            var idProps = SqlManipulationExtensions.GetIdProperties(currenttype).ToList();
            if (!idProps.Any())
                throw new ArgumentException("Entity must have at least one [Key] or Id property");

            var name = SqlManipulationExtensions.GetTableName(currenttype);
            var where = SqlManipulationExtensions.BuildWhere<TEntity>(idProps, entityToDelete);
            string deleteSqlStr = $"delete from {name} where {where}";
            if (DapperDbContext.ThreadLocal_Tag.Value == Enums.TransationWay.UnitOfWork)
            {
                UnitOfWork.AddToUnit(entityToDelete, deleteSqlStr);
                return new Task<int>(() => 0);
            }
            return _connection.ExecuteAsync(deleteSqlStr, entityToDelete, DapperDbContext.GetInnerTransaction());
        }

        public int? Insert(TEntity entityToInsert)
        {
            return Insert<int?>(entityToInsert);
        }

        public TKey Insert<TKey>(TEntity entityToInsert)
        {
            var currenttype = typeof(TEntity);
            var idProps = SqlManipulationExtensions.GetIdProperties(currenttype).ToList();

            if (!idProps.Any())
                throw new ArgumentException("Insert<T> only supports an entity with a [Key] or Id property");

            var keyHasPredefinedValue = false;
            var baseType = typeof(TKey);
            var underlyingType = Nullable.GetUnderlyingType(baseType);
            var keytype = underlyingType ?? baseType;
            if (keytype != typeof(int) && keytype != typeof(uint) && keytype != typeof(long) && keytype != typeof(ulong) && keytype != typeof(short) && keytype != typeof(ushort) && keytype != typeof(Guid) && keytype != typeof(string))
            {
                throw new Exception("Invalid return type");
            }

            var name = SqlManipulationExtensions.GetTableName(currenttype);
            var sb = new StringBuilder();
            sb.AppendFormat("insert into {0}", name);
            sb.Append(" (");
            SqlManipulationExtensions.BuildInsertParameters<TEntity>(sb);
            sb.Append(") ");
            sb.Append("values");
            sb.Append(" (");
            SqlManipulationExtensions.BuildInsertValues<TEntity>(sb);
            sb.Append(")");

            if (keytype == typeof(Guid))
            {
                var guidvalue = (Guid)idProps.First().GetValue(entityToInsert, null);
                if (guidvalue == Guid.Empty)
                {
                    var newguid = SqlManipulationExtensions.SequentialGuid();
                    idProps.First().SetValue(entityToInsert, newguid, null);
                }
                else
                {
                    keyHasPredefinedValue = true;
                }
                sb.Append(";select '" + idProps.First().GetValue(entityToInsert, null) + "' as id");
            }

            if ((keytype == typeof(int) || keytype == typeof(long)) && Convert.ToInt64(idProps.First().GetValue(entityToInsert, null)) == 0)
            {
                sb.Append(";" + base._getIdentitySql);
            }
            else
            {
                keyHasPredefinedValue = true;
            }
            if (DapperDbContext.ThreadLocal_Tag.Value == Enums.TransationWay.UnitOfWork)
            {
                UnitOfWork.AddToUnit(entityToInsert, sb.ToString());
                return (TKey)idProps.First().GetValue(entityToInsert, null);
            }
            var r = _connection.Query(sb.ToString(), entityToInsert, DapperDbContext.GetInnerTransaction(), true);

            if (keytype == typeof(Guid) || keyHasPredefinedValue)
            {
                return (TKey)idProps.First().GetValue(entityToInsert, null);
            }
            return (TKey)r.First().id;
        }

        public async Task<int?> InsertAsync(TEntity t)
        {
            return await InsertAsync<int?>(t);
        }

        public async Task<TKey> InsertAsync<TKey>(TEntity entityToInsert)
        {
            var currenttype = typeof(TEntity);
            var idProps = SqlManipulationExtensions.GetIdProperties(currenttype).ToList();

            if (!idProps.Any())
                throw new ArgumentException("Insert<T> only supports an entity with a [Key] or Id property");

            var keyHasPredefinedValue = false;
            var baseType = typeof(TKey);
            var underlyingType = Nullable.GetUnderlyingType(baseType);
            var keytype = underlyingType ?? baseType;
            if (keytype != typeof(int) && keytype != typeof(uint) && keytype != typeof(long) && keytype != typeof(ulong) && keytype != typeof(short) && keytype != typeof(ushort) && keytype != typeof(Guid) && keytype != typeof(string))
            {
                throw new Exception("Invalid return type");
            }

            var name = SqlManipulationExtensions.GetTableName(currenttype);
            var sb = new StringBuilder();
            sb.AppendFormat("insert into {0}", name);
            sb.Append(" (");
            SqlManipulationExtensions.BuildInsertParameters<TEntity>(sb);
            sb.Append(") ");
            sb.Append("values");
            sb.Append(" (");
            SqlManipulationExtensions.BuildInsertValues<TEntity>(sb);
            sb.Append(")");

            if (keytype == typeof(Guid))
            {
                var guidvalue = (Guid)idProps.First().GetValue(entityToInsert, null);
                if (guidvalue == Guid.Empty)
                {
                    var newguid = SqlManipulationExtensions.SequentialGuid();
                    idProps.First().SetValue(entityToInsert, newguid, null);
                }
                else
                {
                    keyHasPredefinedValue = true;
                }
                sb.Append(";select '" + idProps.First().GetValue(entityToInsert, null) + "' as id");
            }

            if ((keytype == typeof(int) || keytype == typeof(long)) && Convert.ToInt64(idProps.First().GetValue(entityToInsert, null)) == 0)
            {
                sb.Append(";" + base._getIdentitySql);
            }
            else
            {
                keyHasPredefinedValue = true;
            }
            if (DapperDbContext.ThreadLocal_Tag.Value == Enums.TransationWay.UnitOfWork)
            {
                UnitOfWork.AddToUnit(entityToInsert, sb.ToString());
                return (TKey)idProps.First().GetValue(entityToInsert, null);
            }
            var r =await _connection.QueryAsync(sb.ToString(), entityToInsert, DapperDbContext.GetInnerTransaction());

            if (keytype == typeof(Guid) || keyHasPredefinedValue)
            {
                return (TKey)idProps.First().GetValue(entityToInsert, null);
            }
            return (TKey)r.First().id;
        }

        public int Update(TEntity entityToUpdate)
        {
            var currenttype = typeof(TEntity);
            var idProps = SqlManipulationExtensions.GetIdProperties(currenttype).ToList();

            if (!idProps.Any())
                throw new ArgumentException("Entity must have at least one [Key] or Id property");

            var name = SqlManipulationExtensions.GetTableName(currenttype);
            var columns = SqlManipulationExtensions.BuildUpdateSet(entityToUpdate);
            var where = SqlManipulationExtensions.BuildWhere<TEntity>(idProps, entityToUpdate);
            string updateSqlStr = $"update {name} set {columns} where {where}";

            if (DapperDbContext.ThreadLocal_Tag.Value == Enums.TransationWay.UnitOfWork)
            {
                UnitOfWork.AddToUnit(entityToUpdate, updateSqlStr);
                return 0;
            }
            return _connection.Execute(updateSqlStr, entityToUpdate, DapperDbContext.GetInnerTransaction());
        }

        public Task<int> UpdateAsync(TEntity entityToUpdate)
        {
            var currenttype = typeof(TEntity);
            var idProps = SqlManipulationExtensions.GetIdProperties(currenttype).ToList();

            if (!idProps.Any())
                throw new ArgumentException("Entity must have at least one [Key] or Id property");

            var name = SqlManipulationExtensions.GetTableName(currenttype);
            var columns = SqlManipulationExtensions.BuildUpdateSet(entityToUpdate);
            var where = SqlManipulationExtensions.BuildWhere<TEntity>(idProps, entityToUpdate);
            string updateSqlStr = $"update {name} set {columns} where {where}";

            if (DapperDbContext.ThreadLocal_Tag.Value == Enums.TransationWay.UnitOfWork)
            {
                UnitOfWork.AddToUnit(entityToUpdate, updateSqlStr);
                return new Task<int>(() => 0);
            }
            return _connection.ExecuteAsync(updateSqlStr, entityToUpdate, DapperDbContext.GetInnerTransaction());
        }
    }
}
