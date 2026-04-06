public interface IExpensiveService
{
    Task<string> ComputeAsync(string key);
}

public class CachingService
{
    private readonly IExpensiveService inner;
    private Dictionary<string, string> cache = new Dictionary<string, string>();

    public CachingService(IExpensiveService inner)
    {
        this.inner = inner;
    }

    public async ValueTask<string> GetAsync(string key)
    {
        if (cache.TryGetValue(key, out var cachedValue))
        {
            return new ValueTask<string>(cachedValue);
        }
        else
        {
            var computedValue = await inner.ComputeAsync(key).ConfigureAwait(false);
            cache[key] = computedValue;
            return computedValue;
        }
    }
}