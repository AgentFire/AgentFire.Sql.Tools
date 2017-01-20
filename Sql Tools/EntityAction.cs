using System.Data.Linq;

namespace AgentFire.Sql.Tools
{
    public delegate void EntityAction<TDbContext, T>(TDbContext context, T entity)
        where TDbContext : DataContext
        where T : class;
}
