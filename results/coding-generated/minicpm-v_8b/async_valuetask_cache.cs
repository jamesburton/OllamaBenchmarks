using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IExpensiveService
{
    Task<string> ComputeAsync(string key);
}

public class CachingService : IExpensiveService
{
    private readonly Dictionary<string, string> _cache = new Dictionary<string, string>();

    public async ValueTask<string> GetAsync(string key)
    {
        if (_cache.TryGetValue(key, out var cachedValue))
            return cachedValue;

        var result = await Inner.ComputeAsync(key);
        _cache[key] = result;
        return result;
    }

    private readonly IExpensiveService _inner;

    public CachingService(IExpensiveService inner)
    {
        Inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    internal IExpensiveService Inner { get; }
}