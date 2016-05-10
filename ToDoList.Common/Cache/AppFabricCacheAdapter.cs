
using System.Diagnostics;

namespace ToDoList.Common.Cache.Distributed
{
    using Microsoft.ApplicationServer.Caching;
    using Microsoft.Practices.ObjectBuilder2;
    using ToDoList.Common;
    using ToDoList.Common.Cache;


    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Caching;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization; // DO NOT REMOVE! Needed in RELEASE build in method AddItemOfAnyType!
 

    /// <summary>
    /// A cache adapter which supports static/global and instance based caches using the AppFabricCache framework as the cache backend.
    /// </summary>
    public sealed class AppFabricCacheAdapter : AbstractCacheAdapter<DataCache>
    {
        private const string LOG_ERR_ADD_FAILED = "Could not add item of type {0} with key \"{3}\" to cache \"{1}\" (#{2}). There seems to be a problem with the AppFabric cache cluster!";
        private const string LOG_ERR_ADD_FAILED_WITH_REGION = "Could not add item of type {0} with region.key \"{4}\".\"{3}\" to cache \"{1}\" (#{2}). There seems to be a problem with the AppFabric cache cluster!";
        private const string LOG_ERR_ADDORGET_FAILED = "Could not add or get existing item of type {0} with key \"{3}\" to cache \"{1}\" (#{2}). There seems to be a problem with the AppFabric cache cluster!";
        private const string LOG_ERR_ADDORGET_FAILED_WITH_REGION = "Could not add or get existing item of type {0} with region.key \"{4}\".\"{3}\" to cache \"{1}\" (#{2}). There seems to be a problem with the AppFabric cache cluster!";
        private const string LOG_ERR_COUNT_FAILED = "Could not get item count from cache \"{0}\" (#{1}). There seems to be a problem with the AppFabric cache cluster!";
        private const string LOG_ERR_FLUSH_FAILED = "Could not completely flush cache \"{0}\" (#{1}). There seems to be a problem with the AppFabric cache cluster!";
        private const string LOG_ERR_FLUSH_FAILED_WITH_UNKNOWN_EXCEPTION = "Cache \"{0}\" (#{1}) could not be flushed because of an unknown exception!";
        private const string LOG_ERR_GET_FAILED = "Could not get item with key \"{0}\" from cache \"{1}\" (#{2}). There seems to be a problem with the AppFabric cache cluster!";
        private const string LOG_ERR_GET_FAILED_WITH_REGION = "Could not get item with region.key \"{1}\".\"{0}\" from cache \"{2}\" (#{3}). There seems to be a problem with the AppFabric cache cluster!";

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
                    ConsumeOrRethrowIfNotSupported(ex);

                   Debug.WriteLine("Could not generate ID for cache \"{0}\" because cache could not be instantiated! Check cache cluster status!", ex, CacheName);
                    return -1;
                }
            }
        }

        public AppFabricCacheAdapter(ECacheScope cacheScope, string cacheName)
            : base(ECacheType.AppFabric, cacheScope, cacheName)
        {
#if DEBUG
            // Do not use configuration from "EnterpriseLibCaching.config", but use same values except for hosts/servers.
            var fullyQualifiedLocalMachineName = System.Net.Dns.GetHostEntry(Environment.MachineName).HostName; // Results in e.g. "zmucmsappdev02.wacker.corp".
            var dataCacheConfig = new DataCacheFactoryConfiguration("default")
            {
                ChannelOpenTimeout = TimeSpan.FromSeconds(/*15*/1),
                LocalCacheProperties = new DataCacheLocalCacheProperties(100000, TimeSpan.FromSeconds(/*300*/1), DataCacheLocalCacheInvalidationPolicy.TimeoutBased),
                MaxConnectionsToServer = 10,
                NotificationProperties = new DataCacheNotificationProperties(100, TimeSpan.FromSeconds(60)),
                SecurityProperties = new DataCacheSecurity(DataCacheSecurityMode.None, DataCacheProtectionLevel.None),
                Servers = new List<DataCacheServerEndpoint> { new DataCacheServerEndpoint(fullyQualifiedLocalMachineName, 22233) },
                TransportProperties = new DataCacheTransportProperties()
                {
                    ChannelInitializationTimeout = TimeSpan.FromSeconds(/*60*/1),
                    ConnectionBufferSize = 131072,
                    MaxBufferPoolSize = 33554432,
                    MaxBufferSize = 16777216,
                    MaxOutputDelay = TimeSpan.FromSeconds(/*2*/1),
                    ReceiveTimeout = TimeSpan.FromSeconds(/*900000*/5)
                }
            };

            var ctorParams = new object[] { dataCacheConfig };
#else
            var ctorParams = new object[] {/*Empty in order to force unity to use default c'tor for DataCacheFactory!*/ };
#endif
            DependencyFactory.RegisterTypeIfMissing<DataCacheFactory, DataCacheFactory>(true, ctorParams);
        }

        private void AddItemOfAnyType<T>(T item, CacheItemPolicy policy, string key, string region = null)
        {
           // Require.That(() => policy).IsNotNull();

            // For compatiblity reasons with MemoryCache null values shouldn't be stored in AppFabric DataCache.
            if (ReferenceEquals(item, null)) // It is ensured, that value types are given as Nullable<T>, so this should be OK.
            {
                return;
            }

            string policyString;
            TimeSpan timeout;
            if (0 < policy.SlidingExpiration.Ticks)
            {
                policyString = string.Format(EXPIRATION_SLIDING_TEMPLATE, policy.SlidingExpiration.TotalSeconds, policy.Priority);
                timeout = policy.SlidingExpiration;
            }
            else
            {
                policyString = string.Format(EXPIRATION_ABSOLUTE_TEMPLATE, policy.AbsoluteExpiration, policy.Priority);

                // Need to calculate time span for sliding expiration from the given absolute date, because absolute expiration is not supported by AppFabric.
                var ticks = policy.AbsoluteExpiration.UtcTicks - DateTimeOffset.UtcNow.UtcTicks;
                if (ticks < 0)
                {
                    ticks = 0;
                }
                timeout = TimeSpan.FromTicks(ticks);
            }

            try
            {
                if (Contains(key, region))
                {
                    if (string.IsNullOrEmpty(region))
                    { Cache.Put(key, item, timeout); }
                    else
                    { Cache.Put(key, item, timeout, region); }
                    Debug.WriteLine(LOG_DBG_REPLACED_DATA_IN_CACHE_X_WITH_KEY_Y_AND_POLICY_Z, CacheName, CacheId, key, policyString);
                }
                else
                {
                    if (string.IsNullOrEmpty(region))
                    { Cache.Add(key, item, timeout); }
                    else
                    { Cache.Add(key, item, timeout, region); }
                    Debug.WriteLine(LOG_DBG_ADDED_DATA_TO_CACHE_X_WITH_KEY_Y_AND_POLICY_Z, CacheName, CacheId, key, policyString);
                }
            }
#if DEBUG
            catch (Exception ex)
            {
                Debug.WriteLine("Rethrowing exception in DEBUG mode!", ex);
                throw;
            }
#else
            catch (InvalidDataContractException sEx)
            {
                string msg = string.IsNullOrEmpty(region) ? "Could not add item of type {0} with key \"{3}\" to cache \"{1}\" (#{2}) because its structure is not serializable!" : "Could not add item of type {0} with region.key \"{4}\".\"{3}\" to cache \"{1}\" (#{2}) because its structure is not serializable!";
               Debug.WriteLine(msg, sEx, typeof(T).FullName, CacheName, CacheId, key, region);
            }
#endif
        }

        #region Implementation of AbstractCacheAdapter:

        protected override DataCache Cache
        {
            get
            {
                if (CacheBackingStore != null)
                {
                    return CacheBackingStore;
                }

                switch (CacheScope)
                {
                    case ECacheScope.Instance:
                    case ECacheScope.StaticGlobal:
                        var dataCacheFactory = DependencyFactory.Resolve<DataCacheFactory>();
                        CacheBackingStore = dataCacheFactory.GetCache(CacheName);
                        Debug.WriteLine("AppFabric cache \"{0}\" (#{1}) created.", CacheName, CacheId);
                        break;

                    default:
                        throw new NotSupportedException(string.Format(LOG_INF_CACHE_SCOPE_X_NOT_SUPPORTED, CacheScope));
                }

                return CacheBackingStore;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            Debug.WriteLine(LOG_DBG_DISPOSING_CACHE_ADAPTER_X, CacheName);
        }

        public override void Remove(IList<string> keys, string region = null)
        {
            if (keys == null || !keys.Any())
            {
                return;
            }

            var isCacheServerAvailable = true;
            keys.TakeWhile(currCacheKey => isCacheServerAvailable).ForEach(currCacheKey =>
            {
                try
                {
                    if (string.IsNullOrEmpty(region))
                    {
                        Cache.Remove(currCacheKey);
                    }
                    else
                    {
                        Cache.Remove(currCacheKey, region);
                    }
                }
                catch (Exception ex)
                {
                    if (ex is UriFormatException || ex is DataCacheException)
                    {
                        isCacheServerAvailable = false;
                    }
                   Debug.WriteLine(LOG_ERR_REMOVING_ITEM_WITH_KEY_X_FROM_CACHE_Y_FAILED, ex, currCacheKey, CacheName, CacheId);
                }
            });
        }

        #endregion

        #region Override implementations of AbstractCacheAdapter:

        public override long Count
        {
            get
            {
                try
                {
                    return base.Count;
                }
                catch (Exception ex)
                {
                    ConsumeOrRethrowIfNotSupported(ex);

                   Debug.WriteLine(LOG_ERR_COUNT_FAILED, ex, CacheName, CacheId);
                    return 0;
                }
            }
        }

        public override void Add<T>(T item, CacheItemPolicy policy, string key, string region = null)
        {
            try
            {
                base.Add(item, policy, key, region);
            }
            catch (Exception ex)
            {
                ConsumeOrRethrowIfNotSupported(ex);

                if (string.IsNullOrEmpty(region))
                {
                   Debug.WriteLine(LOG_ERR_ADD_FAILED, ex, typeof(T).FullName, CacheName, CacheId, key);
                }
                else
                {
                   Debug.WriteLine(LOG_ERR_ADD_FAILED_WITH_REGION, ex, typeof(T).FullName, CacheName, CacheId, key, region);
                }
            }
        }

        public override void Add<T>(T item, DateTime absoluteExpiry, string key, string region = null)
        {
            try
            {
                base.Add(item, absoluteExpiry, key, region);
            }
            catch (Exception ex)
            {
                ConsumeOrRethrowIfNotSupported(ex);

                if (string.IsNullOrEmpty(region))
                {
                   Debug.WriteLine(LOG_ERR_ADD_FAILED, ex, typeof(T).FullName, CacheName, CacheId, key);
                }
                else
                {
                   Debug.WriteLine(LOG_ERR_ADD_FAILED_WITH_REGION, ex, typeof(T).FullName, CacheName, CacheId, key, region);
                }
            }
        }

        public override void Add<T>(T item, TimeSpan slidingExpiry, string key, string region = null)
        {
            try
            {
                base.Add(item, slidingExpiry, key, region);
            }
            catch (Exception ex)
            {
                ConsumeOrRethrowIfNotSupported(ex);

                if (string.IsNullOrEmpty(region))
                {
                   Debug.WriteLine(LOG_ERR_ADD_FAILED, ex, typeof(T).FullName, CacheName, CacheId, key);
                }
                else
                {
                   Debug.WriteLine(LOG_ERR_ADD_FAILED_WITH_REGION, ex, typeof(T).FullName, CacheName, CacheId, key, region);
                }
            }
        }

        public override void Add<T>(T? item, CacheItemPolicy policy, string key, string region = null)
        {
            try
            {
                base.Add(item, policy, key, region);
            }
            catch (Exception ex)
            {
                ConsumeOrRethrowIfNotSupported(ex);

                if (string.IsNullOrEmpty(region))
                {
                   Debug.WriteLine(LOG_ERR_ADD_FAILED, ex, typeof(T).FullName, CacheName, CacheId, key);
                }
                else
                {
                   Debug.WriteLine(LOG_ERR_ADD_FAILED_WITH_REGION, ex, typeof(T).FullName, CacheName, CacheId, key, region);
                }
            }
        }

        public override void Add<T>(T? item, DateTime absoluteExpiry, string key, string region = null)
        {
            try
            {
                base.Add(item, absoluteExpiry, key, region);
            }
            catch (Exception ex)
            {
                ConsumeOrRethrowIfNotSupported(ex);

                if (string.IsNullOrEmpty(region))
                {
                   Debug.WriteLine(LOG_ERR_ADD_FAILED, ex, typeof(T).FullName, CacheName, CacheId, key);
                }
                else
                {
                   Debug.WriteLine(LOG_ERR_ADD_FAILED_WITH_REGION, ex, typeof(T).FullName, CacheName, CacheId, key, region);
                }
            }
        }

        public override void Add<T>(T? item, TimeSpan slidingExpiry, string key, string region = null)
        {
            try
            {
                base.Add(item, slidingExpiry, key, region);
            }
            catch (Exception ex)
            {
                ConsumeOrRethrowIfNotSupported(ex);

                if (string.IsNullOrEmpty(region))
                {
                   Debug.WriteLine(LOG_ERR_ADD_FAILED, ex, typeof(T).FullName, CacheName, CacheId, key);
                }
                else
                {
                   Debug.WriteLine(LOG_ERR_ADD_FAILED_WITH_REGION, ex, typeof(T).FullName, CacheName, CacheId, key, region);
                }
            }
        }

        public override T AddOrGetExisting<T>(Func<T> itemSource, CacheItemPolicy policy, string key, string region = null)
        {
            try
            {
                return base.AddOrGetExisting(itemSource, policy, key, region);
            }
            catch (Exception ex)
            {
                ConsumeOrRethrowIfNotSupported(ex);

                if (string.IsNullOrEmpty(region))
                {
                   Debug.WriteLine(LOG_ERR_ADDORGET_FAILED, ex, typeof(T).FullName, CacheName, CacheId, key);
                }
                else
                {
                   Debug.WriteLine(LOG_ERR_ADDORGET_FAILED_WITH_REGION, ex, typeof(T).FullName, CacheName,
                        CacheId, key, region);
                }

                return itemSource.Invoke();
            }
        }

        public override T AddOrGetExisting<T>(Func<T> itemSource, DateTime absoluteExpiry, string key, string region = null)
        {
            try
            {
                return base.AddOrGetExisting(itemSource, absoluteExpiry, key, region);
            }
            catch (Exception ex)
            {
                ConsumeOrRethrowIfNotSupported(ex);

                if (string.IsNullOrEmpty(region))
                {
                   Debug.WriteLine(LOG_ERR_ADDORGET_FAILED, ex, typeof(T).FullName, CacheName, CacheId, key);
                }
                else
                {
                   Debug.WriteLine(LOG_ERR_ADDORGET_FAILED_WITH_REGION, ex, typeof(T).FullName, CacheName, CacheId, key, region);
                }

                return itemSource.Invoke();
            }
        }

        public override T AddOrGetExisting<T>(Func<T> itemSource, TimeSpan slidingExpiry, string key, string region = null)
        {
            try
            {
                return base.AddOrGetExisting(itemSource, slidingExpiry, key, region);
            }
            catch (Exception ex)
            {
                ConsumeOrRethrowIfNotSupported(ex);

                if (string.IsNullOrEmpty(region))
                {
                   Debug.WriteLine(LOG_ERR_ADDORGET_FAILED, ex, typeof(T).FullName, CacheName, CacheId, key);
                }
                else
                {
                   Debug.WriteLine(LOG_ERR_ADDORGET_FAILED_WITH_REGION, ex, typeof(T).FullName, CacheName, CacheId, key, region);
                }

                return itemSource?.Invoke();
            }
        }

        public override bool Contains(string key, string region = null)
        {
            return InternalContains(key, region);
        }

        public override void Flush()
        {
            try
            {
                InternalFlush();
            }
            catch (Exception ex)
            {
                ConsumeOrRethrowIfNotSupported(ex);

               Debug.WriteLine(LOG_ERR_FLUSH_FAILED, ex, CacheName, CacheId);
            }
        }

        public override T Get<T>(string key, string region = null)
        {
            try
            {
                return InternalGet<T>(key, region);
            }
            catch (Exception ex)
            {
                ConsumeOrRethrowIfNotSupported(ex);

                if (string.IsNullOrEmpty(region))
                {
                   Debug.WriteLine(LOG_ERR_GET_FAILED, ex, key, CacheName, CacheId);
                }
                else
                {
                   Debug.WriteLine(LOG_ERR_GET_FAILED_WITH_REGION, ex, key, region, CacheName, CacheId);
                }

                return default(T);
            }
        }

        #endregion

        #region Implementation of internal methods defined in AbstractCacheAdapter:

        protected override long InternalCount
        {
            get
            {
                return Cache.GetSystemRegions().Sum(region => Cache.GetObjectsInRegion(region).LongCount());
            }
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
            try
            {
                if (string.IsNullOrEmpty(region))
                {
                    return (Cache.Get(key) != null);
                }

                return Cache.GetObjectsInRegion(region).Any(keyValuePair => keyValuePair.Key == key);
            }
            catch (Exception ex)
            {
                ConsumeOrRethrowIfNotSupported(ex);

                return false;
            }
        }

        protected override void InternalFlush()
        {
            if (0 == InternalCount)
            {
                Debug.WriteLine(LOG_INF_CACHE_X_IS_EMPTY_FLUSHING_UNNECESSARY, CacheName, CacheId);
                return;
            }

            Debug.WriteLine(LOG_INF_CACHE_X_IS_BEING_FLUSHED, CacheName, CacheId);

            try
            {
                // If in Azure, currently this throws an exception and therefore clearing the cache is currently not available in Azure.
                foreach (var regionName in Cache.GetSystemRegions())
                {
                    Cache.ClearRegion(regionName);
                }

                // TODO: If no regions are available in cache, how to get all keys so Remove(keyList) is possible?

                Debug.WriteLine(LOG_INF_CACHE_X_HAS_BEEN_FLUSHED, CacheName, CacheId);
            }
            catch (Exception ex)
            {
                ConsumeOrRethrowIfNotSupported(ex);

               Debug.WriteLine(LOG_ERR_FLUSH_FAILED_WITH_UNKNOWN_EXCEPTION, ex, CacheName, CacheId);
            }
        }

        protected override T InternalGet<T>(string key, string region = null)
        {
            return string.IsNullOrEmpty(region) ? (T)Cache.Get(key) : (T)Cache.Get(key, region);
        }

        #endregion

        private static void ConsumeOrRethrowIfNotSupported(Exception ex)
        {
            if (!(ex is UriFormatException) && !(ex is DataCacheException))
            {
                throw ex;
            }
        }
    }
}