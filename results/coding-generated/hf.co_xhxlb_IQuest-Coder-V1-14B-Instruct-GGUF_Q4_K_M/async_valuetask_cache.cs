using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IExpensiveService
{
    Task<string> ComputeAsync(string key);
}

public class CachingService : IExpensiveService
{
    private readonly IExpensiveService _inner;
    private readonly Dictionary<string, string> _cache = new Dictionary<string, string>();

    public CachingService(IExpensiveService inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    public async ValueTask<string> GetAsync(string key)
    {
        if (_cache.TryGetValue(key, out var cachedValue))
        {
            return new ValueTask<string>(cachedValue);
        }

        var result = await _inner.ComputeAsync(key);
        _cache[key] = result;
        return result;
    }

    public Task<string> ComputeAsync(string key)
    {
        return GetAsync(key);
    }
}