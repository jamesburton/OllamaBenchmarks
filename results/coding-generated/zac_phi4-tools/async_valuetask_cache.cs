using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IExpensiveService
{
    Task<string> ComputeAsync(string key);
}

public class CachingService
{
    private readonly IExpensiveService _inner;
    private readonly Dictionary<string, string> _cache = new();

    public CachingService(IExpensiveService inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    public ValueTask<string> GetAsync(string key)
    {
        if (_cache.TryGetValue(key, out var cachedValue))
        {
            return new ValueTask<string>(cachedValue);
        }

        var result = _inner.ComputeAsync(key).Result;
        _cache[key] = result;
        return new ValueTask<string>(result);
    }
}