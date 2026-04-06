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

    public async ValueTask<string> GetAsync(string key)
    {
        if (_cache.TryGetValue(key, out var cachedValue))
        {
            return new ValueTask<string>(cachedValue);
        }

        var result = await _inner.ComputeAsync(key);

        // Store the result (we assume the result string is the value to cache)
        _cache[key] = result;

        return new ValueTask<string>(result);
    }
}