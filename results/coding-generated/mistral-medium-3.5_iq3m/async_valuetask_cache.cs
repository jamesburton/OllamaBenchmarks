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
        if (cache.TryGetValue(key, out var cachedValue))
        {
            return new ValueTask<string>(cachedValue);
        }

        return new ValueTask<string>(ComputeAndCacheAsync(key));
    }

    private async Task<string> ComputeAndCacheAsync(string key)
    {
        var result = await inner.ComputeAsync(key);
        cache[key] = result;
        return result;
    }
}