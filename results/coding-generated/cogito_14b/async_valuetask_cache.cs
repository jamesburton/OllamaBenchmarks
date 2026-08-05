public interface IExpensiveService {
    Task<string> ComputeAsync(string key);
}

public class CachingService : IDisposable {
    private readonly Dictionary<string, string> _cache = new();
    private readonly IExpensiveService _inner;

    public CachingService(IExpensiveService inner) => _inner = inner ?? throw new ArgumentNullException(nameof(inner));

    public async ValueTask<string> GetAsync(string key) {
        if (_cache.TryGetValue(key, out var cachedValue)) {
            return new ValueTask<string>(cachedValue);
        }

        cachedValue = await _inner.ComputeAsync(key).ConfigureAwait(false);
        _cache[key] = cachedValue;
        return new ValueTask<string>(cachedValue);
    }

    public void Dispose() => _cache.Clear();
}