public interface IExpensiveService {
    Task<string> ComputeAsync(string key);
}

public class CachingService : IExpensiveService {
    private readonly Dictionary<string, string> _cache = new();
    public Task<string> ComputeAsync(string key) => GetAsync(key);
    public async ValueTask<string> GetAsync(string key) {
        if (_cache.TryGetValue(key, out var cachedValue)) {
            return new ValueTask<string>(cachedValue);
        }
        string result = await inner.ComputeAsync(key);
        _cache[key] = result;
        return result;
    }
}