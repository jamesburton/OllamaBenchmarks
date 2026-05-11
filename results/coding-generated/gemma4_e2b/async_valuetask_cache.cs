public interface IExpensiveService
{
    Task<string> ComputeAsync(string key);
}

public class CachingService
{
    private readonly IExpensiveService _inner;
    private readonly Dictionary<string, string> _cache = new Dictionary<string, string>();

    public CachingService(IExpensiveService inner)
    {
        _inner = inner;
    }

    public ValueTask<string> GetAsync(string key)
    {
        if (_cache.TryGetValue(key, out string cachedValue))
        {
            // Cache Hit: Return synchronously
            return new ValueTask<string>(cachedValue);
        }

        // Cache Miss: Compute the value
        Task<string> computationTask = _inner.ComputeAsync(key);

        // Await the result
        string computedValue = computationTask.GetAwaiter().GetResult();

        // Store the result
        _cache[key] = computedValue;

        // Return the result wrapped in ValueTask
        return new ValueTask<string>(computedValue);
    }
}