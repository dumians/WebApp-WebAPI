using System;
using System.Collections.Generic;
using System.Runtime.Caching;

namespace ToDoList.Common.Cache
{
    public abstract class AbstractCacheAdapter<TCache> : ICacheAdapter//ICacheManager
    {
        protected const string EXPIRATION_SLIDING_TEMPLATE = "Sliding expiration in seconds {0}, Priority={1}";
        protected const string EXPIRATION_ABSOLUTE_TEMPLATE = "Absolute expiration at {0}, Priority={1}";
        protected const string LOG_DBG_ADDED_DATA_TO_CACHE_X_WITH_KEY_Y_AND_POLICY_Z = "Added data to cache \"{0}\" (#{1}) with key \"{2}\" and policy: {3}";
        protected const string LOG_DBG_DISPOSING_CACHE_ADAPTER_X = "Disposing cache adapter \"{0}\".";
        protected const string LOG_DBG_REPLACED_DATA_IN_CACHE_X_WITH_KEY_Y_AND_POLICY_Z = "Replaced data in cache \"{0}\" (#{1}) with key \"{2}\" and policy: {3}";
        protected const string LOG_ERR_REMOVING_ITEM_WITH_KEY_X_FROM_CACHE_Y_FAILED = "Unexpected error on removing item with key \"{0}\" from cache \"{1}\" (#{2}).";
        protected const string LOG_ERR_REMOVING_ITEM_WITH_KEY_X_FROM_CACHE_Y_FAILED_WHILE_FLUSHING = "Unexpected error on removing item with key \"{0}\" from cache \"{1}\" (#{2}) while flushing.";
        protected const string LOG_INF_CACHE_SCOPE_X_NOT_SUPPORTED = "Cache scope {0} is not supported!";
        protected const string LOG_INF_CACHE_X_HAS_BEEN_FLUSHED = "Cache \"{0}\" (#{1}) has been flushed.";
        protected const string LOG_INF_CACHE_X_IS_BEING_FLUSHED = "Cache \"{0}\" (#{1}) is being flushed.";
        protected const string LOG_INF_CACHE_X_IS_EMPTY_FLUSHING_UNNECESSARY = "Cache \"{0}\" (#{1}) is empty, no need for flushing.";

       
        private readonly object _getOrAddLock = new object();

        protected TCache CacheBackingStore { get; set; }

        protected abstract TCache Cache { get; }

        protected AbstractCacheAdapter(ECacheType cacheType, ECacheScope cacheScope, string cacheName)
        {
          
            CacheType = cacheType;
            CacheName = cacheName;
            CacheScope = cacheScope;
        }

        protected AbstractCacheAdapter( ECacheScope cacheScope, string cacheName)
        {
         
            
            CacheName = cacheName;
            CacheScope = cacheScope;
        }

        protected abstract void Dispose(bool disposing);

        /// <summary>
        /// <para>Gets the item identified by the given key/region pair.</para>
        /// If no such item exists in the cache the given callback functions are invoked, their result is added to the cache and then returned.
        /// </summary>
        /// <typeparam name="T">The type of the item.</typeparam>
        /// <param name="itemSource">The callback function providing the information how to get the item if it is not in the cache yet.</param>
        /// <param name="policySource">The callback function providing the cache policy to use in case the item needs to be added to the cache.</param>
        /// <param name="key">The key of the item to get.</param>
        /// <param name="region">The region to get the item from.</param>
        /// <returns>The item identified by the given key/region pair or retrieved by the given callback functions.</returns>
        protected T InternalAddOrGetExisting<T>(Func<T> itemSource, Func<CacheItemPolicy> policySource,
            string key, string region = null) where T : class
        {
            // Optimistically trying to get the cached item in order to avoid (expensive) locking.
            var result = InternalGet<T>(key, region);
            if (result != null)
            {
                return result;
            }

            // Optimism didn't work out, so it's time for locking now.
            lock (_getOrAddLock)
            {
                result = InternalGet<T>(key, region);
                if (result == null) // Double check to make sure no other thread already added an item for the key.
                {
                    result = itemSource.Invoke();
                    InternalAdd(result, policySource.Invoke(), key, region);

                    // Use the following block to find recursive calls in debug mode.
                    //var recursivityCheckResult = Get<T>(key, region);
                    //if (recursivityCheckResult == null)
                    //{
                    //    Add(result, policySource.Invoke(), key, region);
                    //}
                }

                return result;
            }
        }

        #region Implementation of ICacheAdapter:

        public string CacheName { get; protected set; }

        public ECacheScope CacheScope { get; protected set; }

 
        public ECacheType CacheType { get; protected set; }

        public virtual long Count
        {
            get { return InternalCount; }
        }

        public virtual void Add<T>(T item, CacheItemPolicy policy, string key, string region = null) where T : class
        {
            InternalAdd(item, policy, key, region);
        }

        public virtual void Add<T>(T item, DateTime absoluteExpiry, string key, string region = null) where T : class
        {
            var policy = new CacheItemPolicy { Priority = CacheItemPriority.Default, AbsoluteExpiration = new DateTimeOffset(absoluteExpiry) };

            InternalAdd(item, policy, key, region);
        }

        public virtual void Add<T>(T item, TimeSpan slidingExpiry, string key, string region = null) where T : class
        {
          
            var policy = new CacheItemPolicy { Priority = CacheItemPriority.Default, SlidingExpiration = slidingExpiry };

            InternalAdd(item, policy, key, region);
        }

        public virtual void Add<T>(T? item, CacheItemPolicy policy, string key, string region = null) where T : struct
        {
            InternalAdd(item, policy, key, region);
        }

        public virtual void Add<T>(T? item, DateTime absoluteExpiry, string key, string region = null) where T : struct
        {
            var policy = new CacheItemPolicy { Priority = CacheItemPriority.Default, AbsoluteExpiration = new DateTimeOffset(absoluteExpiry) };

            InternalAdd(item, policy, key, region);
        }

        public virtual void Add<T>(T? item, TimeSpan slidingExpiry, string key, string region = null) where T : struct
        {
           
            var policy = new CacheItemPolicy { Priority = CacheItemPriority.Default, SlidingExpiration = slidingExpiry };

            InternalAdd(item, policy, key, region);
        }

        public virtual T AddOrGetExisting<T>(Func<T> itemSource, CacheItemPolicy policy, string key, string region = null) where T : class
        {
            Func<CacheItemPolicy> policySource = () => policy;

            return InternalAddOrGetExisting(itemSource, policySource, key, region);
        }

        public virtual T AddOrGetExisting<T>(Func<T> itemSource, DateTime absoluteExpiry, string key, string region = null) where T : class
        {
            Func<CacheItemPolicy> policySource = () => new CacheItemPolicy { Priority = CacheItemPriority.Default, AbsoluteExpiration = absoluteExpiry };

            return InternalAddOrGetExisting(itemSource, policySource, key, region);
        }

        public virtual T AddOrGetExisting<T>(Func<T> itemSource, TimeSpan slidingExpiry, string key, string region = null) where T : class
        {
            Func<CacheItemPolicy> policySource = () => new CacheItemPolicy { Priority = CacheItemPriority.Default, SlidingExpiration = slidingExpiry };

            return InternalAddOrGetExisting(itemSource, policySource, key, region);
        }

        public virtual bool Contains(string key, string region = null)
        {
            return InternalContains(key, region);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Flush()
        {
            InternalFlush();
        }

        public virtual T Get<T>(string key, string region = null)
        {
            return InternalGet<T>(key, region);
        }

        public void Remove(string key, string region = null)
        {
            Remove(new List<string> { key }, region);
        }

        #endregion

        #region Abstract declarations to be implemented by sub classes:

        public abstract void Remove(IList<string> keys, string region = null);

        #endregion

        #region Abstract declarations for internal methods to be implemented by sub classes:

        protected abstract long InternalCount { get; }

        protected abstract void InternalAdd<T>(T item, CacheItemPolicy policy, string key, string region = null) where T : class;

        protected abstract void InternalAdd<T>(T? item, CacheItemPolicy policy, string key, string region = null) where T : struct;

        protected abstract bool InternalContains(string key, string region = null);

        protected abstract void InternalFlush();

        protected abstract T InternalGet<T>(string key, string region = null);

        #endregion
    }
}