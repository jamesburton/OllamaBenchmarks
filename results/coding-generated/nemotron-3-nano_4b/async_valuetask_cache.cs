interface IExpensiveService
{
    Task<string> ComputeAsync(string key);
}

public class CachingService
{
    private readonly IExpensiveService _inner;
    private readonly Dictionary<string, string> _cache = new();

    public CachingService(IExpensiveService inner) => _inner = inner;

    public ValueTask<string> GetAsync(string key)
    {
        if (_cache.TryGetValue(key, out var cachedVal))
            return new ValueTask<string>(cachedVal);

        var task = _inner.ComputeAsync(key).ConfigureAwait(false);
        var result = task.Result;
        _cache[key] = result;
        return new ValueTask<string>(result);
    }
}