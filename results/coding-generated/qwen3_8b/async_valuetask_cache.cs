interface IExpensiveService
{
    Task<string> ComputeAsync(string key);
}

class CachingService
{
    private readonly Dictionary<string, string> _cache = new Dictionary<string, string>();
    private readonly IExpensiveService _inner;

    public CachingService(IExpensiveService inner)
    {
        _inner = inner;
    }

    public async ValueTask<string> GetAsync(string key)
    {
        if (_cache.TryGetValue(key, out var cachedValue))
        {
            return new ValueTask<string>(cachedValue);
        }

        var result = await _inner.ComputeAsync(key);
        _cache[key] = result;
        return new ValueTask<string>(result);
    }
}