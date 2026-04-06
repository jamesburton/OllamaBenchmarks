public interface IExpensiveService
{
    Task<string> ComputeAsync(string key);
}

public class CachingService
{
    private readonly IExpensiveService _inner;
    private readonly System.Collections.Generic.Dictionary<string, string> _cache;

    public CachingService(IExpensiveService inner)
    {
        _inner = inner;
        _cache = new System.Collections.Generic.Dictionary<string, string>();
    }

    public async ValueTask<string> GetAsync(string key)
    {
        if (_cache.TryGetValue(key, out string cachedValue))
        {
            return new ValueTask<string>(cachedValue);
        }

        string result = await _inner.ComputeAsync(key);
        _cache[key] = result;
        return result;
    }
}