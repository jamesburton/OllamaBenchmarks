using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncCacheLib
{
    /// <summary>
    /// A thread-safe asynchronous cache that prevents cache stampede for concurrent requests.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the cache.</typeparam>
    /// <typeparam name="TValue">The type of the values in the cache.</typeparam>
    public class AsyncValueCache<TKey, TValue> where TKey : notnull
    {
        // Holds the cache entries. ConcurrentDictionary ensures thread-safe additions and removals.
        private readonly ConcurrentDictionary<TKey, CacheEntry> _cache;

        /// <summary>
        /// Gets the number of entries currently held in the cache.
        /// </summary>
        public int Count => _cache.Count;

        public AsyncValueCache()
        {
            _cache = new ConcurrentDictionary<TKey, CacheEntry>();
        }

        /// <summary>
        /// Gets the value associated with the specified key, or creates it using the provided factory if it does not exist.
        /// This method ensures that the factory is invoked only once for concurrent calls on the same key (no cache stampede).
        /// </summary>
        /// <param name="key">The key of the value to get or add.</param>
        /// <param name="factory">The factory function to create the value if it is not cached.</param>
        /// <param name="ct">A cancellation token to observe.</param>
        /// <returns>A task that represents the asynchronous operation, containing the cached or newly created value.</returns>
        public async Task<TValue> GetOrAddAsync(
            TKey key,
            Func<TKey, CancellationToken, Task<TValue>> factory,
            CancellationToken ct)
        {
            // Fast path: check if the entry already exists.
            if (_cache.TryGetValue(key, out var existingEntry))
            {
                return await existingEntry.GetValueAsync().ConfigureAwait(false);
            }

            // Slow path: create a new entry.
            // We create the entry instance first to capture the factory logic.
            var newEntry = new CacheEntry(key, factory, ct);

            // Try to add the entry to the dictionary.
            // TryAdd returns true if the key was not present and the entry was added.
            if (_cache.TryAdd(key, newEntry))
            {
                // We won the race. This thread is responsible for invoking the factory.
                return await newEntry.GetValueAsync().ConfigureAwait(false);
            }
            else
            {
                // Another thread won the race and added an entry for this key.
                // We retrieve their entry and await its result.
                if (_cache.TryGetValue(key, out var winningEntry))
                {
                    return await winningEntry.GetValueAsync().ConfigureAwait(false);
                }
                else
                {
                    // Edge case: The entry was added and then immediately removed (e.g., by Clear() or TryRemove).
                    // We retry the operation recursively.
                    return await GetOrAddAsync(key, factory, ct).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Gets the value associated with the specified key, or creates it using the provided factory if it does not exist or has expired.
        /// </summary>
        /// <param name="key">The key of the value to get or add.</param>
        /// <param name="factory">The factory function to create the value if it is not cached or expired.</param>
        /// <param name="ttl">The time-to-live for the cache entry. Entries older than this are evicted on read.</param>
        /// <param name="ct">A cancellation token to observe.</param>
        /// <returns>A task that represents the asynchronous operation, containing the cached or newly created value.</returns>
        public async Task<TValue> GetOrAddWithTtlAsync(
            TKey key,
            Func<TKey, CancellationToken, Task<TValue>> factory,
            TimeSpan ttl,
            CancellationToken ct)
        {
            // Fast path: check if entry exists.
            if (_cache.TryGetValue(key, out var existingEntry))
            {
                // Check if the entry has expired based on the provided TTL.
                if (existingEntry.HasExpired(ttl))
                {
                    // Optimistic removal: try to remove the expired entry.
                    // We use a CAS (Compare-And-Swap) style removal to ensure we only remove the specific expired instance.
                    // This prevents removing a fresh entry that another thread might have just added.
                    _cache.TryRemove(new KeyValuePair<TKey, CacheEntry>(key, existingEntry));
                }
                else
                {
                    // Entry exists and is valid.
                    return await existingEntry.GetValueAsync().ConfigureAwait(false);
                }
            }

            // Entry did not exist, was expired, or was just removed.
            // We call the standard GetOrAddAsync to handle the creation and stampede protection.
            return await GetOrAddAsync(key, factory, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Attempts to remove the entry with the specified key from the cache.
        /// </summary>
        /// <param name="key">The key of the entry to remove.</param>
        /// <returns>true if the entry was successfully removed; otherwise, false.</returns>
        public bool TryRemove(TKey key)
        {
            return _cache.TryRemove(key, out _);
        }

        /// <summary>
        /// Removes all entries from the cache.
        /// </summary>
        public void Clear()
        {
            _cache.Clear();
        }

        /// <summary>
        /// Internal wrapper class for cache entries.
        /// Handles the lifecycle of the value creation task to ensure only one factory invocation.
        /// </summary>
        private class CacheEntry
        {
            private readonly Lazy<Task<TValue>> _lazyValue;
            private readonly DateTime _creationTime;

            // We capture the key and factory for the lazy invocation.
            // Note: We capture 'ct' here for the factory invocation. 
            // If the original request is cancelled, the factory receives the cancellation.
            public CacheEntry(TKey key, Func<TKey, CancellationToken, Task<TValue>> factory, CancellationToken ct)
            {
                _creationTime = DateTime.UtcNow;
                // Lazy<T> ensures that the factory is invoked only once, even under high concurrency.
                _lazyValue = new Lazy<Task<TValue>>(() => factory(key, ct));
            }

            /// <summary>
            /// Gets the task responsible for producing the value.
            /// </summary>
            public Task<TValue> GetValueAsync()
            {
                return _lazyValue.Value;
            }

            /// <summary>
            /// Checks if the entry has exceeded the specified time-to-live.
            /// </summary>
            public bool HasExpired(TimeSpan ttl)
            {
                return DateTime.UtcNow - _creationTime > ttl;
            }
        }
    }
}