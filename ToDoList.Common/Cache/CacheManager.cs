using System.Diagnostics;
using ToDoList.Common.Cache;
using ToDoList.Common.Cache.Distributed;

namespace ToDoList.Common.Cache
{
   
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Runtime.Caching;
 

    [Serializable]
    public class CacheManager : ICacheManager
    {
       // private readonly ILog _logger = LogManager.GetLogger(LogCategories.Caching);


        private const string APP_SETTINGS_CACHE_MANAGER_BACKEND_TYPE_MAP = "CacheManagerBackendTypeMap";

        /// <summary>
        /// Parses the configured mappings and gets the cache backend type for the given cache name.
        /// </summary>
        /// <param name="cacheName">The name of the cache to get the configured cache backend type for.</param>
        /// <returns></returns>
        private static ECacheType GetCacheTypeByName(string cacheName)
        {
            const ECacheType defaultValue = ECacheType.Memory;

            if (string.IsNullOrEmpty(cacheName))
            {
                return defaultValue;
            }

            var settings = ConfigurationManager.GetSection(APP_SETTINGS_CACHE_MANAGER_BACKEND_TYPE_MAP) as NameValueCollection;
            if (settings == null || settings[cacheName] == null)
            {
                return defaultValue;
            }

            return ECacheTypeExtensions.GetByName(settings[cacheName]);
        }

         /// <summary>
        /// Creates a <b>CacheManager</b> instance using the given scope and name for the managed cache.
        /// </summary>
        /// <param name="cacheScope">The cache scope to use.</param>
        /// <param name="cacheName">The name of the cache. Must not be <code>null</code> or empty.</param>
        public CacheManager(ECacheScope cacheScope, string cacheName)
            : this(CreateCacheAdapter(cacheScope, cacheName))
        {
        }

        protected CacheManager(ICacheAdapter cacheAdapter)
        {
            CacheAdapter = cacheAdapter;
        }

        /// <summary>
        /// Creates the underlying cache adapter depending on the cache framework configured in the App settings.
        /// </summary>
        private static ICacheAdapter CreateCacheAdapter(ECacheScope cacheScope, string cacheName)
        {
            var cacheType = GetCacheTypeByName(cacheName);

            switch (cacheType)
            {
                case ECacheType.AppFabric:
                    return new AppFabricCacheAdapter(cacheScope, cacheName);

                default/*ECacheType.Memory*/:
              
                    return new MemoryCacheAdapter(cacheScope, cacheName);
            }
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                //CacheAdapter.Dispose();
                Debug.WriteLine("Dispose CacheManager for cache with name \"{0}\".", CacheName);
            }
        }

        #region Implementation of ICacheManager:

        public ICacheAdapter CacheAdapter { get; private set; }

        #endregion

        #region Implementation of ICacheAdapter:

        public string CacheName
        {
            get { return CacheAdapter.CacheName; }
        }

        public ECacheScope CacheScope
        {
            get { return CacheAdapter.CacheScope; }
        }

        public ECacheType CacheType
        {
            get { return CacheAdapter.CacheType; }
        }

        public long Count
        {
            get { return CacheAdapter.Count; }
        }

        public void Add<T>(T item, CacheItemPolicy policy, string key, string region = null) where T : class
        {
            CacheAdapter.Add(item, policy, key, region);
        }

        public void Add<T>(T item, DateTime absoluteExpiry, string key, string region = null) where T : class
        {
            CacheAdapter.Add(item, absoluteExpiry, key, region);
        }

        public void Add<T>(T item, TimeSpan slidingExpiry, string key, string region = null) where T : class
        {
            CacheAdapter.Add(item, slidingExpiry, key, region);
        }

        public void Add<T>(T? item, CacheItemPolicy policy, string key, string region = null) where T : struct
        {
            CacheAdapter.Add(item, policy, key, region);
        }

        public void Add<T>(T? item, DateTime absoluteExpiry, string key, string region = null) where T : struct
        {
            CacheAdapter.Add(item, absoluteExpiry, key, region);
        }

        public void Add<T>(T? item, TimeSpan slidingExpiry, string key, string region = null) where T : struct
        {
            CacheAdapter.Add(item, slidingExpiry, key, region);
        }

        public void AddWithDefaultSlidingTime<T>(T itemSource, string key) where T : class
        {
             CacheAdapter.Add(itemSource, TimeSpan.FromHours(4), key);
        }

   

        public T AddOrGetExisting<T>(Func<T> itemSource, CacheItemPolicy policy, string key, string region = null) where T : class
        {
            return CacheAdapter.AddOrGetExisting(itemSource, policy, key, region);
        }

        public T AddOrGetExisting<T>(Func<T> itemSource, DateTime absoluteExpiry, string key, string region = null) where T : class
        {
            return CacheAdapter.AddOrGetExisting(itemSource, absoluteExpiry, key, region);
        }

        public T AddOrGetExisting<T>(Func<T> itemSource, TimeSpan slidingExpiry, string key, string region = null) where T : class
        {
            return CacheAdapter.AddOrGetExisting(itemSource, slidingExpiry, key, region);
        }

       

        public bool Contains(string key, string region = null)
        {
            return CacheAdapter.Contains(key, region);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Flush()
        {
            CacheAdapter.Flush();
        }

        public T Get<T>(string key, string region = null)
        {
            return CacheAdapter.Get<T>(key, region);
        }

        public void Remove(string key, string region = null)
        {
            CacheAdapter.Remove(key, region);
        }

        public void Remove(IList<string> keys, string region = null)
        {
            CacheAdapter.Remove(keys, region);
        }

        #endregion
    }
}