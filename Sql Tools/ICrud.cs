using System;
using System.Data.Linq;
using System.Linq.Expressions;

namespace AgentFire.Sql.Tools
{
    public interface ICrud<TDbContext> where TDbContext : DataContext
    {
        T Get<T>(int id) where T : class;

        /// <summary>
        /// Works only if there is exactly one INT primary key on the Linq2Sql entity. Use Create{T, TResult} if you have composite primary key.
        /// Returns the INT primary key of the created entity.
        /// </summary>
        int Create<T>(EntityAction<TDbContext, T> initializer) where T : class, new();
        TResult Create<T, TResult>(EntityAction<TDbContext, T> initializer, Func<T, TResult> afterInsert) where T : class, new();

        bool TryDelete<T>(int id) where T : class;

        /// <summary>
        /// Returns amount of items deleted.
        /// </summary>
        int TryDelete<T>(Expression<Func<T, bool>> predicateExpression) where T : class;

        bool Modify<T>(int id, EntityAction<TDbContext, T> modifier) where T : class;

        /// <summary>
        /// Returns amount of items modified.
        /// </summary>
        /// <returns></returns>
        int Modify<T>(Expression<Func<T, bool>> predicateExpression, EntityAction<TDbContext, T> modifier) where T : class;

        bool CreateOrModify<T>(Expression<Func<T, bool>> predicateExpression, EntityAction<TDbContext, T> createActions, EntityAction<TDbContext, T> modifier) where T : class, new();
    }
}
