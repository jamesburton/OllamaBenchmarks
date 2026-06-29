public interface IExpensiveService {
    System.Threading.Tasks.Task<string> ComputeAsync(string key);
}

public class CachingService {
    private readonly System.Collections.Generic.Dictionary<string, string> cache = new();
    private readonly IExpensiveService inner;

    public CachingService(IExpensiveService inner) {
        this.inner = inner;
    }

    public System.Threading.Tasks.ValueTask<string> GetAsync(string key) {
        if (cache.TryGetValue(key, out var cachedValue)) {
            return new System.Threading.Tasks.ValueTask<string>(cachedValue);
        }

        var result = await inner.ComputeAsync(key);
        cache[key] = result;
        return new System.Threading.Tasks.ValueTask<string>(result);
    }
}