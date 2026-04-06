public interface IExpensiveService
{
    Task<string> ComputeAsync(string key);
}

public class CachingService : IExpensiveService
{
    private readonly IExpensiveService inner;
    private readonly Dictionary<string, string> cache = new Dictionary<string, string>();

    public CachingService(IExpensiveService inner)
    {
        this.inner = inner;
    }

    public async ValueTask<string> GetAsync(string key)
    {
        if (cache.TryGetValue(key, out string cachedValue))
        {
            return new ValueTask<string>(cachedValue);
        }

        string result = await inner.ComputeAsync(key);
        cache[key] = result;
        return new ValueTask<string>(result);
    }
}