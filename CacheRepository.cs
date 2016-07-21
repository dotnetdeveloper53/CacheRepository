using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CacheRepository
{
    public class CacheRepository<TEntity, TKey> : IRepository<TEntity, TKey>
        where TEntity : class
    {
        readonly IRepository<TEntity, TKey> _repository;
        readonly ConcurrentDictionary<string, object> _lockers = new ConcurrentDictionary<string, object>();
        readonly MemoryCache _cache;
        readonly CacheItemPolicy _cachePolicy;

        public CacheRepository(IRepository<TEntity, TKey> repository)
            : this(repository, new TimeSpan(0, 5, 0))
        {
        }

        public CacheRepository(IRepository<TEntity, TKey> repository
            , TimeSpan slidingEntityExpiration)
        {
            if (repository == null)
                throw new ArgumentNullException("repository");

            _repository = repository;
            _cache = MemoryCache.Default;

            _cachePolicy = new CacheItemPolicy()
            {
                SlidingExpiration = slidingEntityExpiration,
                RemovedCallback = entry =>
                {
                    /*
                     * when an object is removed from the cache it
                     * would be sensible to keep the locker collection
                     * in sync so that we don't hold unnecessary lock
                     * objects for entities which no longer exist
                     */
                    object removedEntity;
                    _lockers.TryRemove(entry.CacheItem.Key, out removedEntity);
                }
            };
        }

        void SetCachedEntity(string key, TEntity entity)
        {
            /*
             * if we're updating an entity in the cache
             * we want to remove the original first
             */
            _cache.Remove(key);
            if (entity != null)
                _cache.Add(key, entity, _cachePolicy);
        }

        TEntity GetCachedEntity(string key)
        {
            return (TEntity)_cache.Get(key);
        }

        object GetLocker(string key)
        {
            return _lockers.GetOrAdd(key, new object());
        }

        public virtual TEntity Get(TKey key)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            string keystr = key.ToString();

            TEntity cachedEntity;
            lock (GetLocker(keystr))
            {
                //try and retrieve the entity from the local cache
                cachedEntity = GetCachedEntity(keystr);
                if (cachedEntity == null)
                {
                    //object doesn't exist - therefore try and load it from the underlying repository    
                    cachedEntity = _repository.Get(key);
                    if (cachedEntity != null)
                    {
                        //entity loaded successfully therefore set it in the local cache
                        SetCachedEntity(keystr, cachedEntity);
                    }
                }
            }

            return cachedEntity;
        }

        public void Save(TKey key, TEntity entity)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            string keystr = key.ToString();

            lock (GetLocker(keystr))
            {
                /*
                 * try and persist the entity to the underlying
                 * repository prior to updating the local cache
                 */
                _repository.Save(key, entity);

                //save was successful therefore update the local cache
                SetCachedEntity(keystr, entity);
            }
        }
    }
}
