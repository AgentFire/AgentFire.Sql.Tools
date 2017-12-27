using System;
using System.Data.Linq;
using System.Linq.Expressions;

namespace AgentFire.Sql.Tools
{
    public interface ICrud<TDbContext> where TDbContext : DataContext
    {
        T Get<T>(int id) where T : class;
        int Create<T>(EntityAction<TDbContext, T> initializer) where T : class, new();

        bool TryDelete<T>(int id) where T : class;
        int TryDelete<T>(Expression<Func<T, bool>> predicateExpression) where T : class;

        bool Modify<T>(int id, EntityAction<TDbContext, T> modifier) where T : class;
        int Modify<T>(Expression<Func<T, bool>> predicateExpression, EntityAction<TDbContext, T> modifier) where T : class;
    }
}
