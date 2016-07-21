using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheRepository
{
    public class CloneCacheRepository<TEntity, TKey> : CacheRepository<TEntity, TKey>
        where TEntity : class, IClone<TEntity>
    {
        public CloneCacheRepository(IRepository<TEntity, TKey> repository)
            : base(repository, new TimeSpan(0, 5, 0))
        {
        }

        public CloneCacheRepository(IRepository<TEntity, TKey> repository
            , TimeSpan slidingEntityExpiration)
            : base(repository, slidingEntityExpiration)
        {
        }

        public override TEntity Get(TKey key)
        {
            var entity = base.Get(key);
            return entity != null ? entity.DeepClone() : entity;
        }
    }
}
