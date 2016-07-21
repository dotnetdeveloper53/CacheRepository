using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheRepository
{
    public interface IClone<T>
    {
        T DeepClone();
    }

    public interface IRepository<TEntity, TKey> : IQueryRepository<TEntity, TKey>, IPersistableRepository<TEntity, TKey>
    {
    }

    public interface IQueryRepository<TEntity, TKey>
    {
        TEntity Get(TKey key);
    }

    public interface IPersistableRepository<TEntity, TKey>
    {
        void Save(TKey key, TEntity entity);
    }
}
