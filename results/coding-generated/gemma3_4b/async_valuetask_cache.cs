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
        if (_cache.TryGetValue(key, out string cachedValue))
        {
            return new ValueTask<string>(cachedValue);
        }

        var result = _inner.ComputeAsync(key);
        _cache[key] = result.Result;
        return result;
    }
}