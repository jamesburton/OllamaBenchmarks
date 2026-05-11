interface IExpensiveService
{
    Task<string> ComputeAsync(string key);
}

class CachingService
{
    private readonly System.Collections.Generic.Dictionary<string, string> _cache = new System.Collections.Generic.Dictionary<string, string>();
    private readonly IExpensiveService _inner;

    public CachingService(IExpensiveService inner)
    {
        _inner = inner;
    }

    public async System.Threading.Tasks.ValueTask<string> GetAsync(string key)
    {
        if (_cache.TryGetValue(key, out var cachedValue))
        {
            return new System.Threading.Tasks.ValueTask<string>(cachedValue);
        }
        var result = await _inner.ComputeAsync(key);
        _cache[key] = result;
        return new System.Threading.Tasks.ValueTask<string>(result);
    }
}