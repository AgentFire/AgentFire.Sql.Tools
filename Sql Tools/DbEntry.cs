using System;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace AgentFire.Sql.Tools
{
    public abstract class DbEntry<TDbContext> : DbEntry where TDbContext : DataContext, new()
    {
        public static ICrud<TDbContext> Crud { get; } = new CrudImpl();
        public new TDbContext Context { get; }

        #region Crud Impl

        private sealed class CrudImpl : ICrud<TDbContext>
        {
            #region PK Finder

            private static class Cache<T>
            {
                public static Lazy<PropertyInfo> LazyProp { get; } = new Lazy<PropertyInfo>(Initializer, LazyThreadSafetyMode.ExecutionAndPublication);

                private static PropertyInfo Initializer()
                {
                    var query = from property in typeof(T).GetProperties()
                                let attr = property.GetCustomAttributes<ColumnAttribute>().Where(T => T.IsPrimaryKey).FirstOrDefault()
                                where attr != null
                                where attr.IsPrimaryKey
                                select property;

                    return query.SingleOrDefault();
                }
            }

            private static PropertyInfo GetPrimaryKeyFor<TEntity>() => Cache<TEntity>.LazyProp.Value;

            #endregion
            #region Expression Cache

            private static class ExpressionCache<T>
            {
                public static ParameterExpression Parameter { get; } = Expression.Parameter(typeof(T), "Entity");
                public static MemberExpression IDProperty { get; } = Expression.Property(Parameter, GetPrimaryKeyFor<T>());
                public static Func<T, int> IDPropertyCompiled { get; } = Expression.Lambda<Func<T, int>>(IDProperty, Parameter).Compile();
            }

            #endregion

            private static Expression<Func<T, bool>> GetIdSelector<T>(int id)
            {
                BinaryExpression check = Expression.Equal(ExpressionCache<T>.IDProperty, Expression.Constant(id));
                return Expression.Lambda<Func<T, bool>>(check, ExpressionCache<T>.Parameter);
            }

            public bool TryDelete<T>(int id) where T : class
            {
                using (DbEntry db = new DbEntry(EntryMode.Automatic, new TDbContext()))
                {
                    Table<T> table = db.Context.GetTable<T>();
                    T entity = table.Where(GetIdSelector<T>(id)).SingleOrDefault();

                    if (entity == null)
                    {
                        return false;
                    }

                    table.DeleteOnSubmit(entity);
                }

                return true;
            }

            public int Create<T>(EntityAction<TDbContext, T> initializer) where T : class, new()
            {
                T entity = new T();

                TDbContext context = new TDbContext();
                using (DbEntry db = new DbEntry(EntryMode.Automatic, context))
                {
                    Table<T> table = db.Context.GetTable<T>();
                    initializer(context, entity);
                    table.InsertOnSubmit(entity);
                } // Automatic SubmitChanges() will write 'id' property into the entity instance.

                return ExpressionCache<T>.IDPropertyCompiled(entity);
            }
            public bool Modify<T>(int id, EntityAction<TDbContext, T> modifier) where T : class
            {
                TDbContext context = new TDbContext();

                using (DbEntry db = new DbEntry(EntryMode.Automatic, context))
                {
                    Table<T> table = db.Context.GetTable<T>();
                    T entity = table.Where(GetIdSelector<T>(id)).SingleOrDefault();

                    if (entity == null)
                    {
                        return false;
                    }

                    modifier(context, entity);
                }

                return true;
            }

            public T Get<T>(int id) where T : class
            {
                return PickData(db => db.GetTable<T>().Where(GetIdSelector<T>(id)).SingleOrDefault());
            }
        }

        #endregion

        protected DbEntry(EntryMode mode) : base(mode, new TDbContext())
        {
            Context = (TDbContext)base.Context;
        }

        public static T PickData<T>(Func<TDbContext, T> selector)
        {
            T result;

            using (TDbContext context = new TDbContext())
            {
                result = selector(context);
            }

            return result;
        }
        public static T[] PickDataToArray<T>(Func<TDbContext, IQueryable<T>> selector)
        {
            return PickData(db => selector(db).ToArray());
        }
    }

    public class DbEntry : DisposableObject
    {
        private static readonly Random _random = new Random();
        public const int SqlDeadlockErrorCode = 1205;

        public EntryMode Mode { get; }
        public DataContext Context { get; }

        public DbEntry(EntryMode mode, DataContext context)
        {
            Mode = mode;
            Context = context;
        }

        public void Submit()
        {
            Context.SubmitChanges();
        }

        public async Task DeadlockAwareSubmit(int retryCount = 3, int minDelay = 300, int randomizedDelay = 600)
        {
            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    Context.SubmitChanges();
                    return;
                }
                catch (SqlException ex) when (ex.Number == SqlDeadlockErrorCode)
                {
                    await Task.Delay(minDelay + _random.Next(randomizedDelay));
                    continue;
                }
                /*catch (ChangeConflictException)
                {
                    if (swallowChangeConflicts)
                    {
                        d.ChangeConflicts.ResolveAll(RefreshMode.OverwriteCurrentValues);
                        return;
                    }

                    throw;
                }*/
            }
        }

        protected override void DisposeInternal()
        {
            if (Mode == EntryMode.Automatic)
            {
                Submit();
            }

            Context.Dispose();
        }
    }
}
