using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IExpensiveService
{
    Task<string> ComputeAsync(string key);
}

public class CachingService
{
    private readonly IExpensiveService inner;
    private readonly Dictionary<string, string> cache = new Dictionary<string, string>();

    public CachingService(IExpensiveService inner)
    {
        this.inner = inner;
    }

    public ValueTask<string> GetAsync(string key)
    {
        if (cache.TryGetValue(key, out string cachedValue))
        {
            return new ValueTask<string>(cachedValue);
        }

        return new ValueTask<string>(FetchAndCache(key));
    }

    private async Task<string> FetchAndCache(string key)
    {
        string result = await inner.ComputeAsync(key);
        cache[key] = result;
        return result;
    }
}