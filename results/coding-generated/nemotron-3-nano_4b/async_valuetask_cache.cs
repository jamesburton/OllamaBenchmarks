interface IExpensiveService
{
    Task<string> ComputeAsync(string key);
}

class CachingService
{
    private IExpensiveService inner;
    private Dictionary<string, string> cache = new();

    public CachingService(IExpensiveService inner) => this.inner = inner;
    public ValueTask<string> GetAsync(string key)
    {
        return cache.TryGetValue(key, out var cachedValue) ? new ValueTask<string>(cachedValue) : inner.ComputeAsync(key).ContinueWith(work => cache[key] = work.Result).ToValueTask().ConfigureAwait(false);
    }
}