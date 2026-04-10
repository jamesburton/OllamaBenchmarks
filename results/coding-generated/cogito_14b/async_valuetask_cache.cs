public interface IExpensiveService
{
    Task<string> ComputeAsync(string key);
}

public class CachingService
{
    private readonly IExpensiveService _inner;
    private readonly Dictionary<string, string> _cache;

    public CachingService(IExpensiveService inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _cache = new Dictionary<string, string>();
    }

    public async ValueTask<string> GetAsync(string key)
    {
        if (_cache.TryGetValue(key, out string cachedValue))
        {
            return new ValueTask<string>(cachedValue);
        }

        cachedValue = await _inner.ComputeAsync(key);
        _cache[key] = cachedValue;
        return new ValueTask<string>(cachedValue);
    }
}