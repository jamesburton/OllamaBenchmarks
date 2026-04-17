using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class AsyncCache<TKey, TValue>
{
    private readonly Dictionary<TKey, TValue> _cache;
    private readonly SemaphoreSlim _lock;

    public AsyncCache()
    {
        _cache = new Dictionary<TKey, TValue>();
        _lock = new SemaphoreSlim(1, 1);
    }

    public async Task<TValue> GetOrAddAsync(TKey key, Func<TKey, Task<TValue>> factory)
    {
        // First, check if the value exists under the lock.
        await _lock.WaitAsync();
        try
        {
            if (_cache.TryGetValue(key, out var cachedValue))
            {
                return cachedValue;
            }
        }
        finally
        {
            _lock.Release();
        }

        // If not found, execute the factory method outside the lock to allow concurrency.
        TValue value = await factory(key);

        // Acquire the lock again to add the value to the dictionary.
        await _lock.WaitAsync();
        try
        {
            // Check if the key was added by another thread in the meantime.
            if (!_cache.ContainsKey(key))
            {
                _cache[key] = value;
            }

            return _cache[key];
        }
        finally
        {
            _lock.Release();
        }
    }
}