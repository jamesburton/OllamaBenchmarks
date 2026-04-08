public interface IExpensiveService
{
    System.Threading.Tasks.Task<string> ComputeAsync(string key);
}

public class CachingService
{
    private readonly System.Collections.Generic.Dictionary<string, string> _cache = new();
    private readonly IExpensiveService _inner;

    public CachingService(IExpensiveService inner)
    {
        _inner = inner;
    }

    public async System.Threading.Tasks.ValueTask<string> GetAsync(string key)
    {
        if (_cache.TryGetValue(key, out var cachedValue))
        {
            return cachedValue;
        }

        string result = await _inner.ComputeAsync(key);
        _cache[key] = result;
        return result;
    }
}