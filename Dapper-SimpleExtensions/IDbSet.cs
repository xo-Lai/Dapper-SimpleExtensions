using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Dapper_SimpleExtensions
{
    public interface IDbSet<TEntity> : IQuery<TEntity>, ICommand<TEntity> where TEntity : class
    {

    }
    public interface IQuery<TResult> where TResult : class
    {

        IQuery<TResult> Where(Expression<Func<TResult, bool>> exp);

        IQuery<TResult> OrderBy(Expression<Func<TResult, object>> exp);

        IQuery<TResult> OrderByDesc(Expression<Func<TResult, object>> exp);

        IEnumerable<TResult> GetListPaged(int pageIndex, int pageSize);

        IEnumerable<TResult> GetList();
        TResult GetFisrt();
        long RecordCount();
        bool Exist();
        TResult Get(object id);
    }
    public interface ICommand<TEntity> where TEntity : class
    {
        TKey Insert<TKey>(TEntity t);
        int? Insert(TEntity t);
        int Update(TEntity t);
        int Delete(TEntity t);
        int Delete(Object id);
    }
}
