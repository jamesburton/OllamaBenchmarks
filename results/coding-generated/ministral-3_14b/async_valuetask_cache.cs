public interface IExpensiveService
{
    Task<string> ComputeAsync(string key);
}

public class CachingService
{
    private readonly IExpensiveService _inner;
    private readonly Dictionary<string, string> _cache = new Dictionary<string, string>();

    public CachingService(IExpensiveService inner)
    {
        _inner = inner;
    }

    public ValueTask<string> GetAsync(string key)
    {
        if (_cache.TryGetValue(key, out var cachedValue))
        {
            return new ValueTask<string>(cachedValue);
        }

        return new ValueTask<string>(ComputeAndCacheAsync(key).GetAwaiter().GetResult());
    }

    private async Task<string> ComputeAndCacheAsync(string key)
    {
        var result = await _inner.ComputeAsync(key);
        _cache[key] = result;
        return result;
    }
}