using System;
using System.Collections.Generic;
using System.Runtime.Caching;

namespace ToDoList.Common.Cache
{
    /// <summary>
    /// Describes the methods and properties which allow cache adapters to add/get items from/to their underlying cache implementation.
    /// </summary>
    public enum ECacheScope
    {
        ChildContainer,
        Instance,
        StaticGlobal
    }

    public interface ICacheManager : IDisposable
    {
        /// <summary>
        /// The cache name property.
        /// </summary>
        string CacheName { get; }

        /// <summary>
        /// The property providing information about the scope of the cache.
        /// </summary>
        ECacheScope CacheScope { get; }

        

        /// <summary>
        /// Returns the number of items currently stored in the cache (includes the items of all regions).
        /// Be aware that counting the cached objects may have a significant performance impact depending on the underlying cache implementation.
        /// So avoid using this property's value, especially when only needed for log output!
        /// </summary>
        long Count { get; }


        /// <summary>
        /// <para>Adds an item to the cache with the given key/region pair. When <code>null</code> is used as item, no changes to the cache's content will be made.</para>
        /// In case the cache already contains an item for the given key it will be overwritten (replaced) with the new one.
        /// </summary>
        /// <typeparam name="T">The type of the cache item.</typeparam>
        /// <param name="item">The item to add to the cache. Using <code>null</code> here is allowed but won't have an effect.</param>
        /// <param name="policy">The policy describing the eviction and expiration details of the item.</param>
        /// <param name="key">The key used to reference the given item within the cache.</param>
        /// <param name="region">The region to store the given item in.</param>
        void Add<T>(T item, CacheItemPolicy policy, string key, string region = null) where T : class;

        /// <summary>
        /// <para>Adds an item to the cache with the given key/region pair. When <code>null</code> is used as item, no changes to the cache's content will be made.</para>
        /// In case the cache already contains an item for the given key it will be overwritten (replaced) with the new one.
        /// </summary>
        /// <typeparam name="T">The type of the cache item.</typeparam>
        /// <param name="item">The item to add to the cache. Using <code>null</code> here is allowed but won't have an effect.</param>
        /// <param name="absoluteExpiry">The point of time at which the item will be invalidated, marked as expired and finally removed from the cache.</param>
        /// <param name="key">The key used to reference the given item within the cache.</param>
        /// <param name="region">The region to store the given item in.</param>
        void Add<T>(T item, DateTime absoluteExpiry, string key, string region = null) where T : class;

        /// <summary>
        /// <para>Adds an item to the cache with the given key/region pair. When <code>null</code> is used as item, no changes to the cache's content will be made.</para>
        /// In case the cache already contains an item for the given key it will be overwritten (replaced) with the new one.
        /// </summary>
        /// <typeparam name="T">The type of the cache item.</typeparam>
        /// <param name="item">The item to add to the cache. Using <code>null</code> here is allowed but won't have an effect.</param>
        /// <param name="slidingExpiry">The time span the item will remain in the cache before it will be invalidated, marked as expired and finally removed from the cache.</param>
        /// <param name="key">The key used to reference the given item within the cache.</param>
        /// <param name="region">The region to store the given item in.</param>
        void Add<T>(T item, TimeSpan slidingExpiry, string key, string region = null) where T : class;

        /// <summary>
        /// <para>Adds a nullable value type item to the cache with the given key/region pair. When <code>null</code> is used as item, no changes to the cache's content will be made.</para>
        /// In case the cache already contains an item for the given key it will be overwritten (replaced) with the new one.
        /// </summary>
        /// <typeparam name="T">The type of the cache item.</typeparam>
        /// <param name="item">The item to add to the cache. Using <code>null</code> here is allowed but won't have an effect.</param>
        /// <param name="policy">The policy describing the eviction and expiration details of the item.</param>
        /// <param name="key">The key used to reference the given item within the cache.</param>
        /// <param name="region">The region to store the given item in.</param>
        void Add<T>(T? item, CacheItemPolicy policy, string key, string region = null) where T : struct;

        /// <summary>
        /// <para>Adds a nullable value type item to the cache with the given key/region pair. When <code>null</code> is used as item, no changes to the cache's content will be made.</para>
        /// In case the cache already contains an item for the given key it will be overwritten (replaced) with the new one.
        /// </summary>
        /// <typeparam name="T">The type of the cache item.</typeparam>
        /// <param name="item">The item to add to the cache. Using <code>null</code> here is allowed but won't have an effect.</param>
        /// <param name="absoluteExpiry">The point of time at which the item will be invalidated, marked as expired and finally removed from the cache.</param>
        /// <param name="key">The key used to reference the given item within the cache.</param>
        /// <param name="region">The region to store the given item in.</param>
        void Add<T>(T? item, DateTime absoluteExpiry, string key, string region = null) where T : struct;

        /// <summary>
        /// <para>Adds a nullable value type item to the cache with the given key/region pair. When <code>null</code> is used as item, no changes to the cache's content will be made.</para>
        /// In case the cache already contains an item for the given key it will be overwritten (replaced) with the new one.
        /// </summary>
        /// <typeparam name="T">The type of the cache item.</typeparam>
        /// <param name="item">The item to add to the cache. Using <code>null</code> here is allowed but won't have an effect.</param>
        /// <param name="slidingExpiry">The time span the item will remain in the cache before it will be invalidated, marked as expired and finally removed from the cache.</param>
        /// <param name="key">The key used to reference the given item within the cache.</param>
        /// <param name="region">The region to store the given item in.</param>
        void Add<T>(T? item, TimeSpan slidingExpiry, string key, string region = null) where T : struct;

        /// <summary>
        /// <para>Gets the item referenced by the given key from the specified region.</para>
        /// <para>If there's no such item in the cache, the given callback function will be used to retrieve the item from its original source and</para>
        /// also store it in the cache using the given policy and the specified key/region pair.
        /// </summary>
        /// <typeparam name="T">The type of the cached item.</typeparam>
        /// <param name="itemSource">The callback function which will be used to retrieve the item in case it is not stored in the cache yet. The funtion may also return <code>null</code>.</param>
        /// <param name="policy">The policy describing the eviction and expiration details of the item.</param>
        /// <param name="key">The key of the item to get.</param>
        /// <param name="region">The region to get the item from.</param>
        /// <returns><para>The cached item referenced by the given key in the specified region.</para>
        /// If no such item exists, the given function will be used to retrieve and cache the item. The result may still be <code>null</code>.</returns>
        T AddOrGetExisting<T>(Func<T> itemSource, CacheItemPolicy policy, string key, string region = null) where T : class;

        /// <summary>
        /// <para>Gets the item referenced by the given key from the specified region.</para>
        /// <para>If there's no such item in the cache, the given callback function will be used to retrieve the item from its original source and</para>
        /// also store it in the cache using the given absolute expiry date and the specified key/region pair.
        /// </summary>
        /// <typeparam name="T">The type of the cached item.</typeparam>
        /// <param name="itemSource">The callback function which will be used to retrieve the item in case it is not stored in the cache yet. The funtion may also return <code>null</code>.</param>
        /// <param name="absoluteExpiry">The point of time at which the item will be invalidated, marked as expired and finally removed from the cache.</param>
        /// <param name="key">The key of the item to get.</param>
        /// <param name="region">The region to get the item from.</param>
        /// <returns><para>The cached item referenced by the given key in the specified region.</para>
        /// If no such item exists, the given function will be used to retrieve and cache the item. The result may still be <code>null</code>.</returns>
        T AddOrGetExisting<T>(Func<T> itemSource, DateTime absoluteExpiry, string key, string region = null) where T : class;

        /// <summary>
        /// <para>Gets the item referenced by the given key from the specified region.</para>
        /// <para>If there's no such item in the cache, the given callback function will be used to retrieve the item from its original source and</para>
        /// also store it in the cache using the given eviction time span and the specified key/region pair.
        /// </summary>
        /// <typeparam name="T">The type of the cached item.</typeparam>
        /// <param name="itemSource">The callback function which will be used to retrieve the item in case it is not stored in the cache yet. The funtion may also return <code>null</code>.</param>
        /// <param name="slidingExpiry">The time span the item will remain in the cache before it will be invalidated, marked as expired and finally removed from the cache.</param>
        /// <param name="key">The key of the item to get.</param>
        /// <param name="region">The region to get the item from.</param>
        /// <returns><para>The cached item referenced by the given key in the specified region.</para>
        /// If no such item exists, the given function will be used to retrieve and cache the item. The result may still be <code>null</code>.</returns>
        T AddOrGetExisting<T>(Func<T> itemSource, TimeSpan slidingExpiry, string key, string region = null) where T : class;



    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    /// <param name="policy"></param>
    /// <param name="key"></param>
        void AddWithDefaultSlidingTime<T>(T item, string key) where T : class;

        /// <summary>
        /// Checks whether the cache contains an item referenced by the given key in the given region.
        /// </summary>
        /// <param name="key">The key of the item to check for.</param>
        /// <param name="region">The region to check. Can be <code>null</code> in order to check the default region.</param>
        /// <returns>Returns <code>true</code> if the cache contains an item matching the given criteria, <code>false</code> otherwise.</returns>
        bool Contains(string key, string region = null);

        /// <summary>
        /// Removes all items from all regions in the cache.
        /// </summary>
        void Flush();

        /// <summary>
        /// Gets the item referenced by the given key from the specified region.
        /// </summary>
        /// <typeparam name="T">The type of the cached item.</typeparam>
        /// <param name="key">The key of the item to get.</param>
        /// <param name="region">The region to get the item from.</param>
        /// <returns><para>The cached item referenced by the given key in the specified region.</para>
        /// If no such item exists, null will be returned.</returns>
        T Get<T>(string key, string region = null);

        /// <summary>
        /// Removes the items referenced by the given list of keys from the specified region.
        /// </summary>
        /// <param name="keys">The list of keys of the items to remove.</param>
        /// <param name="region">The region from which to remove the items.</param>
        void Remove(IList<string> keys, string region = null);

        /// <summary>
        /// Removes the item referenced by the given key from the specified region.
        /// </summary>
        /// <param name="key">The key of the item to remove.</param>
        /// <param name="region">The region from which to remove the item.</param>
        void Remove(string key, string region = null);

    }
}