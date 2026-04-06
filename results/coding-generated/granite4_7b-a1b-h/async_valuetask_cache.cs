interface IExpensiveService {
    Task<string> ComputeAsync(string key);
}

class CachingService {
    private readonly IExpensiveService _inner;
    private readonly Dictionary<string, string> _cache = new();

    public CachingService(IExpensiveService inner) => _inner = inner;

    public async ValueTask<string> GetAsync(string key) {
        if (_cache.TryGetValue(key, out var cachedValue)) {
            return cachedValue;
        }

        var result = await _inner.ComputeAsync(key);
        _cache[key] = result;
        return result;
    }
}