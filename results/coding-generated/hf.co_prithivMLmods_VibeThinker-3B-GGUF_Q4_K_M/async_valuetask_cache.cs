interface IExpensiveService
{
    Task<string> ComputeAsync(string key);
}

class CachingService
{
    private Dictionary<string, string> _cache = new();

    public CachingService(IExpensiveService inner)
    {
        if (!typeof(inner).IsSupertypeof(this))
        {
            throw new TypeError("CachingService must implement the IExpensiveService interface.");
        }
    }

    ValueTask<string> GetAsync(string key)
    {
        if (_cache.TryGetValue(key, out var cachedValue))
        {
            return new ValueTask<string>(cachedValue);
        }

        // Miss: perform the expensive operation
        var result = await inner.ComputeAsync(key);
        _cache[key] = result;
        // Convert the Task result to a ValueTask for consistency
        return new ValueTask<string>(result);
    }
}