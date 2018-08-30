using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Dapper_SimpleExtensions
{
    public interface IDbSet<TEntity> : IQuery<TEntity>, ICommand<TEntity> where TEntity : class
    {

    }
    public interface IQuery<TResult> where TResult : class
    {

        /// <summary>
        /// where conditions
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        IQuery<TResult> Where(Expression<Func<TResult, bool>> exp);

        /// <summary>
        /// orderby conditions
        /// asc
        /// </summary>
        /// <param name="exp">Lambda表达式</param>
        /// <returns></returns>
        IQuery<TResult> OrderBy(Expression<Func<TResult, object>> exp);

        /// <summary>
        /// orderby conditions
        /// desc
        /// </summary>
        /// <param name="exp">Lambda表达式</param>
        /// <returns></returns>
        IQuery<TResult> OrderByDesc(Expression<Func<TResult, object>> exp);

        /// <summary>
        /// Gets a list of entities with optional
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="rowPerpage"></param>
        /// <returns></returns>
        PageData<TResult> GetListPaged(int pageNumber, int rowPerpage);

        Task<PageData<TResult>> GetListPagedAsync(int pageNumber, int rowPerpage);
        /// <summary>
        /// Gets a list of entities
        /// </summary>
        /// <returns></returns>
        IEnumerable<TResult> GetList();

        /// <summary>
        /// Gets a list of entities by async
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<TResult>> GetListAsync();
        /// <summary>
        /// get single entity
        /// </summary>
        /// <returns> Returns a single entity </returns>
        TResult GetFisrt();

        Task<TResult> GetFisrtAsync();
        /// <summary>
        /// Returns a count of records.
        /// </summary>
        /// <returns></returns>
        long RecordCount();


        Task<long> RecordCountAsync();
        /// <summary>
        /// is existence 
        /// </summary>
        /// <returns></returns>
        bool Exist();
        /// <summary>
        /// get single entity by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns> Returns a single entity by a single id from table T. </returns>
        TResult Get(object id);

        Task<TResult> GetAsync(object id);
    }
    public interface ICommand<TEntity> where TEntity : class
    {
        TKey Insert<TKey>(TEntity t);
        int? Insert(TEntity t);
        int Update(TEntity t);
        int Delete(TEntity t);
        int Delete(Object id);

        Task<TKey> InsertAsync<TKey>(TEntity t);
        Task<int?> InsertAsync(TEntity t);
        Task<int> UpdateAsync(TEntity t);
        Task<int> DeleteAsync(TEntity t);
        Task<int> DeleteAsync(Object id);
    }
}
