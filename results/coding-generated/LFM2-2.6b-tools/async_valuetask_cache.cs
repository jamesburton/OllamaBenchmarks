public interface IExpensiveService
{
    Task<string> ComputeAsync(string key);
}

public class CachingService : IExpensiveService
{
    private readonly IExpensiveService _inner;
    private readonly Dictionary<string, string> _cache;

    public CachingService(IExpensiveService inner)
    {
        _inner = inner;
        _cache = new Dictionary<string, string>();
    }

    public async ValueTask<string> ComputeAsync(string key)
    {
        if (_cache.TryGetValue(key, out var cachedValue))
        {
            return new ValueTask<string>(cachedValue);
        }

        var result = await _inner.ComputeAsync(key);
        _cache[key] = result;
        return result;
    }
}