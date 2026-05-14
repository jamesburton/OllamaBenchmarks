interface IExpensiveService
{
    Task<string> ComputeAsync(string key);
}

class CachingService
{
    private readonly IExpensiveService inner;
    private readonly Dictionary<string, string> cache = new Dictionary<string, string>();

    public CachingService(IExpensiveService inner)
    {
        this.inner = inner;
    }

    public ValueTask<string> GetAsync(string key)
    {
        if (cache.TryGetValue(key, out var cachedValue))
        {
            return new ValueTask<string>(cachedValue);
        }

        var task = inner.ComputeAsync(key);
        task.ContinueWith(t =>
        {
            if (t.IsCompletedSuccessfully)
            {
                cache[key] = t.Result;
            }
        });

        return new ValueTask<string>(task);
    }
}