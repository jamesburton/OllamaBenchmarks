using System.Threading.Tasks;

interface IExpensiveService
{
    Task<string> ComputeAsync(string key);
}

class CachingService : IExpensiveService
{
    private readonly IExpensiveService inner;
    private Dictionary<string, string> cache = new();

    public CachingService(IExpensiveService inner)
    {
        this.inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    public ValueTask<string> GetAsync(string key)
    {
        if (string.IsNullOrEmpty(key))
            return new ValueTask<string>(null);

        string cachedValue;
        if (!cache.TryGetValue(key, out cachedValue))
        {
            var result = inner.ComputeAsync(key).AsCompletionSource().TrySetResult(cachedValue);
            if (result)
                cache[key] = cachedValue;

            return default; // Return a completed task with the value from the completion source
        }

        return new ValueTask<string>(cachedValue); // Synchronously unwrap to get the result directly.
    }
}