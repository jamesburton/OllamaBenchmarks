public interface IExpensiveService
{
    Task<string> ComputeAsync(string key);
}

public class CachingService
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

        var result = inner.ComputeAsync(key).GetAwaiter().GetResult();
        cache[key] = result;
        return new ValueTask<string>(result);
    }
}