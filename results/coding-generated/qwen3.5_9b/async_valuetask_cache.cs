public interface IExpensiveService
{
    Task<string> ComputeAsync(string key);
}

public class CachingService
{
    private readonly Dictionary<string, string> _cache = new();
    private readonly IExpensiveService _inner;

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

        return _inner.ComputeAsync(key).ContinueWith(task =>
        {
            _cache[key] = task.Result;
            return task.Result;
        });
    }
}