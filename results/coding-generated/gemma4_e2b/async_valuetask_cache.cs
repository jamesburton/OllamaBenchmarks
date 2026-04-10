public interface IExpensiveService
{
    Task<string> ComputeAsync(string key);
}

public class CachingService
{
    private readonly IExpensiveService inner;
    private readonly System.Collections.Generic.Dictionary<string, string> cache = new System.Collections.Generic.Dictionary<string, string>();

    public CachingService(IExpensiveService inner)
    {
        this.inner = inner;
    }

    public System.Threading.Tasks.ValueTask<string> GetAsync(string key)
    {
        if (cache.TryGetValue(key, out string cachedValue))
        {
            return new System.Threading.Tasks.ValueTask<string>(cachedValue);
        }

        string computedValue = await inner.ComputeAsync(key);
        cache[key] = computedValue;
        return System.Threading.Tasks.ValueTask.FromResult(computedValue);
    }
}