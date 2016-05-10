
namespace ToDoList.Common.Cache
{

    using System;
    using Common;
    using System.Diagnostics;


    /// <summary>
    /// Static factory class used to get/create instances of named cache managers.
    /// </summary>
    public static class CacheFactory
    {
        private static readonly object LockObject = new object();

      //  private static readonly ILog Logger = LogManager.GetLogger(LogCategories.Caching);

        /// <summary>
        /// <para>Returns the named cache manager instance using <b>ECacheScope.Instance</b> as its scope.</para>
        /// Guaranteed to return an initialized ICacheManager if no exception thrown.
        /// </summary>
        /// <param name="cacheName">Name defined in configuration for the cache to instantiate.</param>
        /// <returns>The requested CacheManager instance.</returns>
        /// <exception cref="ArgumentNullException">If cacheName is null.</exception>
        /// <exception cref="ArgumentException">If cacheName is the empty string or "Default".</exception>
        /// <exception cref="System.Configuration.ConfigurationException">Could not find instance specified by cacheName.</exception>
        /// <exception cref="InvalidOperationException">Error processing configuration information defined in application configuration file.</exception>
        public static ICacheManager GetCacheManager(string cacheName)
        {
            return GetCacheManager(ECacheScope.Instance, cacheName);
        }

        /// <summary>
        /// <para>Returns the named cache manager instance.</para>
        /// Guaranteed to return an initialized cache manager if no exception is thrown.
        /// </summary>
        /// <param name="cacheScope">The cache scope to use.</param>
        /// <param name="cacheName">Name defined in configuration for the cache to instantiate.</param>
        /// <returns>The requested CacheManager instance.</returns>
        /// <exception cref="ArgumentNullException">If cacheName is null.</exception>
        /// <exception cref="ArgumentException">If cacheName is the empty string or "Default".</exception>
        /// <exception cref="System.Configuration.ConfigurationException">Could not find instance specified by cacheName.</exception>
        /// <exception cref="InvalidOperationException">Error processing configuration information defined in application configuration file.</exception>
        public static ICacheManager GetCacheManager(ECacheScope cacheScope, string cacheName)
        {
           // Require.That(() => cacheName).IsNotNullOrWhiteSpace();

            lock (LockObject)
            {
                var cacheManager = DependencyFactory.ResolveSafe<ICacheManager>(cacheName) ?? new CacheManager(cacheScope, cacheName);

               Debug.WriteLine("GetCacheManager: Scope={0}, Name=\"{1}\"", cacheScope, cacheName);
                return cacheManager;
            }
        }
    }
}