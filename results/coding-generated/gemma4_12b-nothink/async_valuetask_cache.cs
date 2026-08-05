public interface IExpensiveService
{
    Task<string> ComputeAsync(string key);
}

public class CachingService
{
    private readonly IExpensiveService _inner;
    private readonly System.Collections.Generic.Dictionary<string, string> _cache = new();

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

        var result = _inner.ComputeAsync(key).GetAwaiter().GetResult();
        _cache[key] = result;
        return new ValueTask<string>(result);
    }
}