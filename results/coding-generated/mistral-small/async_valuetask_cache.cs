public interface IExpensiveService
{
    Task<string> ComputeAsync(string key);
}

public class CachingService : IExpensiveService
{
    private readonly IExpensiveService inner;
    private Dictionary<string, string> cache;

    public CachingService(IExpensiveService inner)
    {
        this.inner = inner;
        this.cache = new Dictionary<string, string>();
    }

    public ValueTask<string> GetAsync(string key)
    {
        if (cache.TryGetValue(key, out var cachedValue))
        {
            return new ValueTask<string>(cachedValue);
        }
        else
        {
            return ComputeAndCacheAsync(key);
        }
    }

    private async Task<string> ComputeAndCacheAsync(string key)
    {
        string result = await inner.ComputeAsync(key);
        cache[key] = result;
        return result;
    }

    public Task<string> ComputeAsync(string key) => GetAsync(key).AsTask();
}