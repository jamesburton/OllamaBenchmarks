interface IExpensiveService { Task<string> ComputeAsync(string key); }

class CachingService : IExpensiveService
{
    private Dictionary<string, string> _cache = new Dictionary<string, string>();

    public async Task GetAsync(string key)
    {
        if (_cache.ContainsKey(key))
        {
            return _cache[key];
        }
        else
        {
            var result = await inner.ComputeAsync(key);
            _cache[key] = result;
            return new ValueTask<string>(result);
        }
    }
}