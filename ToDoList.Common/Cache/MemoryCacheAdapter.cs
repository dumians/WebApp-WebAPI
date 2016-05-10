using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Caching;
using System.Runtime.CompilerServices;
using Microsoft.Practices.ObjectBuilder2;

namespace ToDoList.Common.Cache
{
    /// <summary>
    /// A cache adapter which supports static/global and instance based caches using the in-memory caches provided by the MemoryCache framework as the cache backend.
    /// Regions are not supported. All methods providing the optional region parameter will use a default region as fallback.
    /// Support for request specific caches will be added in the future.
    /// </summary>
    public sealed class MemoryCacheAdapter : AbstractCacheAdapter<MemoryCache>
    {
        private static readonly object StaticGlobalCachesLock = new object();

        private static readonly IDictionary<string, MemoryCache> StaticGlobalCaches = new Dictionary<string, MemoryCache>();

        
        private int? _cacheIdBackingStore;

      
        private int CacheId
        {
            get
            {
                if (!ReferenceEquals(_cacheIdBackingStore, null))
                {
                    return _cacheIdBackingStore.Value;
                }

                try
                {
                    if (_cacheIdBackingStore == null)
                    {
                        _cacheIdBackingStore = RuntimeHelpers.GetHashCode(Cache);
                    }

                    return _cacheIdBackingStore.Value;
                }
                catch (Exception ex)
                {
                   Debug.WriteLine(string.Format("Could not generate ID for cache \"{0}\" because cache could not be instantiated!", ex, CacheName));
                    return -1;
                }
            }
        }

        /// <summary>
        /// Creates a new cache adapter for an in memory cache.
        /// </summary>
        /// <param name="cacheScope">The cache scope to use.</param>
        /// <param name="cacheName">The name of the cache. Must not be null or empty.</param>
        /// <param name="isFireCacheContentRemovedDomainEventOnFlush">Optional parameter that determines the behaviour for firing events when the entire cache is flushed.</param>
        /// <param name="isFireCacheContentRemovedDomainEventOnItemUpdate">Optional parameter that determines the behaviour for firing events when individual items are updated or removed.</param>
        public MemoryCacheAdapter(ECacheScope cacheScope, string cacheName)
           : base(ECacheType.Memory, cacheScope, cacheName)
        {
           if (cacheScope == ECacheScope.ChildContainer)
            {
                
                DependencyFactory.RegisterTypeIfMissingHierachical<MemoryCache, MemoryCache>(cacheName, new object[] { cacheName, new NameValueCollection() });
            }

        }

        private void AddItemOfAnyType<T>(T item, CacheItemPolicy policy, string key, string region = null)
        {
          
            if (policy.RemovedCallback == null || !policy.RemovedCallback.GetInvocationList().Contains((CacheEntryRemovedCallback)CacheItemWasRemovedCallback))
            {
                policy.RemovedCallback += CacheItemWasRemovedCallback;
            }

            // Null values can't be stored in MemoryCache, attempting to do so results in an ArgumentNullException.
            if (ReferenceEquals(item, null)) // It is ensured, that value types are given as Nullable<T>, so this should be OK.
            {
                return;
            }

            string policyString;
            if (0 < policy.SlidingExpiration.Ticks)
            {
                policyString = string.Format(EXPIRATION_SLIDING_TEMPLATE, policy.SlidingExpiration.TotalSeconds, policy.Priority);
            }
            else
            {
                policyString = string.Format(EXPIRATION_ABSOLUTE_TEMPLATE, policy.AbsoluteExpiration, policy.Priority);
            }

            var cacheItem = new CacheItem(key, item, region);

            if (Contains(key, region))
            {
                Cache.Set(cacheItem, policy);
                Debug.WriteLine(string.Format(LOG_DBG_REPLACED_DATA_IN_CACHE_X_WITH_KEY_Y_AND_POLICY_Z, CacheName, CacheId, key, policyString));
                //Logger.Debug(Environment.StackTrace);
                FireCacheContentRemovedDomainEvent(new List<string> { key });
            }
            else
            {
                Cache.Add(cacheItem, policy);
                Debug.WriteLine(string.Format(LOG_DBG_ADDED_DATA_TO_CACHE_X_WITH_KEY_Y_AND_POLICY_Z, CacheName, CacheId, key, policyString));
            }

            Debug.WriteLine(string.Format("Cache \"{0}\" currently uses approximately {1} bytes.", CacheName));
        }

        /// <summary>
        /// Callback method for tracking cache item removals. The method is registered for every cache item upon adding it to the cache.
        /// </summary>
        /// <param name="arguments">Provides information about the removed item, cache and reason.</param>
        public void CacheItemWasRemovedCallback(CacheEntryRemovedArguments arguments)
        {
            if (arguments == null || !ReferenceEquals(Cache, arguments.Source))
            {
                return;
            }

            var cacheItemKey = arguments.CacheItem.Key;
            var removedReason = arguments.RemovedReason.ToString();

            Debug.WriteLine(string.Format("The cached item associated with key \"{2}\" was removed from cache \"{0}\" (#{1}). Reason: {3}", CacheName, CacheId, cacheItemKey, removedReason));
        }

        private void FireCacheContentRemovedDomainEvent(IList<string> removedCacheKeys = null)
        {
            if (CacheScope == ECacheScope.ChildContainer)
            {
                return;
            }

            var isItemUpdate = (removedCacheKeys != null && removedCacheKeys.Any());
            if (isItemUpdate ) // Check event behaviour for item update.
            {
                return;
            }

            var isFlush = !isItemUpdate;
           
        }

        //public long GetCurrentApproximateSize()
        //{
        //    return _cacheBackingStoreSizedRef == null ? 0 : _cacheBackingStoreSizedRef.ApproximateSize;
        //}

        #region Implementation of AbstractCacheAdapter:

        protected override MemoryCache Cache
        {
            get
            {
                if (CacheScope == ECacheScope.ChildContainer)
                {
                    if (!DependencyFactory.ChildContainerExists())
                    {
                        throw new InvalidOperationException("There's no ChildContainer available! Requested cache could not be resolved.");
                    }
                    return DependencyFactory.Resolve<MemoryCache>(CacheName);
                }
                if (CacheBackingStore == null)
                {
                    lock (StaticGlobalCachesLock)
                    {
                        if (CacheBackingStore == null)
                        {
                            switch (CacheScope)
                            {
                                case ECacheScope.Instance:
                                    CacheBackingStore = string.Equals("Default", CacheName) ? MemoryCache.Default : new MemoryCache(CacheName);
                                    break;

                                case ECacheScope.ChildContainer:
                                    // TODO: Implement support for request specific caches!
                                    throw new NotSupportedException("Support for request specific caches is not supported/implemented yet!");

                                case ECacheScope.StaticGlobal:
                                    MemoryCache cache;
                                    if (StaticGlobalCaches.TryGetValue(CacheName, out cache))
                                    {
                                        CacheBackingStore = cache;
                                    }
                                    else
                                    {
                                        CacheBackingStore = string.Equals("Default", CacheName) ? MemoryCache.Default : new MemoryCache(CacheName);
                                        StaticGlobalCaches.Add(CacheName, CacheBackingStore);
                                    }
                                    break;
                            }
                        }
                       // _cacheBackingStoreSizedRef = new SizedRef(CacheBackingStore);
                    }
                }

                return CacheBackingStore;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && Cache != null)
            {
                Debug.WriteLine(string.Format(LOG_DBG_DISPOSING_CACHE_ADAPTER_X, CacheName));

                //lock (STATIC_CACHE_VAULT_LOCK)
                //{
                //    MemoryCache cache;
                //    if (STATIC_CACHE_VAULT.TryGetValue(CacheName, out cache))
                //    {
                //        STATIC_CACHE_VAULT.Remove(CacheName);
                //    }
                //}
                //Cache.Dispose();
            }
        }

        public override void Remove(IList<string> keys, string region = null)
        {
            if (keys == null || !keys.Any())
            {
                // Avoid unnecessary creation of domain events and network bus messages (see CacheContentRemovedManager).
                return;
            }

            try
            {
                keys.ForEach(currCacheKey =>
                {
                    try
                    {
                        Cache.Remove(currCacheKey, region);
                    }
                    catch (Exception ex)
                    {
                       Debug.WriteLine(LOG_ERR_REMOVING_ITEM_WITH_KEY_X_FROM_CACHE_Y_FAILED, ex, currCacheKey, CacheName);
                    }
                });
            }
            finally
            {
                FireCacheContentRemovedDomainEvent(keys);
            }
        }

        #endregion

      
        protected override long InternalCount
        {
            get { return Cache.GetCount(); }
        }

       

        protected override void InternalAdd<T>(T item, CacheItemPolicy policy, string key, string region = null)
        {
            AddItemOfAnyType(item, policy, key, region);
        }

        protected override void InternalAdd<T>(T? item, CacheItemPolicy policy, string key, string region = null)
        {
            AddItemOfAnyType(item, policy, key, region);
        }

        protected override bool InternalContains(string key, string region = null)
        {
            return Cache.Contains(key, region);
        }

        protected override void InternalFlush()
        {
            if (0 == Count)
            {
                Debug.WriteLine(string.Format(LOG_INF_CACHE_X_IS_EMPTY_FLUSHING_UNNECESSARY, CacheName, CacheId));
                return;
            }

            Debug.WriteLine(string.Format(LOG_INF_CACHE_X_IS_BEING_FLUSHED, CacheName, CacheId));

                //Cache.Dispose();
            //CacheBackingStore = null; // Signals to create a new cache instance when trying to access the corresponding property.
            //Debug.WriteLine(string.Format(LOG_INF_CACHE_X_HAS_BEEN_FLUSHED, CacheName, cacheId);
            //FireCacheContentRemovedDomainEvent();

            Cache.Trim(100);
            var cacheAsList = Cache.ToList();
            try
            {
                cacheAsList.ForEach(i =>
                {
                    var key = i.Key;

                    // Granular exception around clearing cache items so it can continue clearing if an error occurs.
                    try
                    {
                        Cache.Remove(key);
                    }
                    catch (Exception ex)
                    {
                       Debug.WriteLine(string.Format(LOG_ERR_REMOVING_ITEM_WITH_KEY_X_FROM_CACHE_Y_FAILED_WHILE_FLUSHING, ex, key, CacheName));
                    }
                });

                // FS: This one is quite funny. The trim method keeps cache entries although a 100% of the items are requested to be removed.
                // See http://connect.microsoft.com/VisualStudio/feedback/details/831755/memorycache-trim-method-doesnt-evict-100-of-the-items
                // Cache.Trim(100);
            }
            finally
            {
                Debug.WriteLine(string.Format(LOG_INF_CACHE_X_HAS_BEEN_FLUSHED, CacheName, CacheId));
                FireCacheContentRemovedDomainEvent();
            }
        }

        protected override T InternalGet<T>(string key, string region = null)
        {
            return (T)Cache.Get(key, region);
        }

       
    }
}