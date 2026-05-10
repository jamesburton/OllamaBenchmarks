interface IExpensiveService
{
    Task<string> ComputeAsync(string key);
}

class CachingService : IExpensiveService
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
            return new ValueTask<string>(cachedValue);

        async Task<string> ComputeAndCache()
        {
            string result = await _inner.ComputeAsync(key);
            _cache[key] = result;
            return result;
        }

        return new ValueTask<string>(ComputeAndCache());
    }
}