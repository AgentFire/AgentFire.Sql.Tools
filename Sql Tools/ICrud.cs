using System.Data.Linq;

namespace AgentFire.Sql.Tools
{
    public interface ICrud<TDbContext> where TDbContext : DataContext
    {
        bool TryDelete<T>(int id) where T : class;
        int Create<T>(EntityAction<TDbContext, T> initializer) where T : class, new();
        bool Modify<T>(int id, EntityAction<TDbContext, T> modifier) where T : class;
        T Get<T>(int id) where T : class;
    }
}
