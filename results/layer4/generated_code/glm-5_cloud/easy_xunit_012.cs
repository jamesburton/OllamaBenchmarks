using System;
using System.Collections.Generic;

public interface ICache<TKey, TValue>
{
    void Set(TKey key, TValue value);
    bool TryGet(TKey key, out TValue? value);
}

public class MemoryCache<TKey, TValue> : ICache<TKey, TValue>
{
    private readonly Dictionary<TKey, TValue> _cache = new Dictionary<TKey, TValue>();

    public void Set(TKey key, TValue value)
    {
        _cache[key] = value;
    }

    public bool TryGet(TKey key, out TValue? value)
    {
        return _cache.TryGetValue(key, out value);
    }
}