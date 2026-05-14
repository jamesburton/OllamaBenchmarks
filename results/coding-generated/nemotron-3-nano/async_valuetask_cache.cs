interface IExpensiveService
{
    Task<string> ComputeAsync(string key);
}

class CachingService
{
    private readonly IExpensiveService _inner;
    private readonly Dictionary<string, string> _cache = new();

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

        var result = _inner.ComputeAsync(key);
        _cache[key] = result.Result;
        return new ValueTask<string>(result.Result);
    }
}