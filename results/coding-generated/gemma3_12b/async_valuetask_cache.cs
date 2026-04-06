interface IExpensiveService
{
    Task<string> ComputeAsync(string key);
}

class CachingService
{
    private readonly IExpensiveService _inner;
    private readonly Dictionary<string, string> _cache = new Dictionary<string, string>();

    public CachingService(IExpensiveService inner)
    {
        _inner = inner;
    }

    public ValueTask<string> GetAsync(string key)
    {
        if (_cache.ContainsKey(key))
        {
            string cachedValue = _cache[key];
            return new ValueTask<string>(cachedValue);
        }
        else
        {
            return ComputeAndCacheAsync(key);
        }
    }

    private async ValueTask<string> ComputeAndCacheAsync(string key)
    {
        string result = await _inner.ComputeAsync(key);
        _cache[key] = result;
        return new ValueTask<string>(result);
    }
}