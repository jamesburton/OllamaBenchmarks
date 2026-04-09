public interface IExpensiveService
{
    System.Threading.Tasks.Task<string> ComputeAsync(string key);
}

public class CachingService
{
    private readonly System.Collections.Generic.Dictionary<string, string> _cache;
    private readonly IExpensiveService _inner;

    public CachingService(IExpensiveService inner)
    {
        _inner = inner;
        _cache = new System.Collections.Generic.Dictionary<string, string>();
    }

    public async System.Threading.Tasks.ValueTask<string> GetAsync(string key)
    {
        if (_cache.ContainsKey(key))
        {
            return new System.Threading.Tasks.ValueTask<string>(_cache[key]);
        }

        string result = await _inner.ComputeAsync(key);
        _cache[key] = result;
        return result;
    }
}