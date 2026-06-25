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
        if (cache.TryGetValue(key, out string cachedValue))
        {
            return new ValueTask<string>(cachedValue);
        }

        return inner.ComputeAsync(key).ContinueWith(task =>
        {
            cache[key] = task.Result;
            return task.Result;
        });
    }
}