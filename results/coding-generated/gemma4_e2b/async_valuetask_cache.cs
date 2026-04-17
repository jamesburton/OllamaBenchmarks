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

        string result = System.Threading.Tasks.Task.Run(() => inner.ComputeAsync(key)).Result;
        cache[key] = result;
        return new System.Threading.Tasks.ValueTask<string>(result);
    }
}