public interface IExpensiveService
{
    Task<string> ComputeAsync(string key);
}

public class CachingService
{
    private readonly Dictionary<string, string> _cache = new();
    private readonly IExpensiveService _inner;

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

        return ComputeAndCacheAsync(key);
    }

    private async Task<ValueTask<string>> ComputeAndCacheAsync(string key)
    {
        string result = await _inner.ComputeAsync(key).ConfigureAwait(false);
        _cache[key] = result;
        return new ValueTask<string>(result);
    }
}