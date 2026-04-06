public interface IExpensiveService
        {
            Task<string> ComputeAsync(string key);
        }

public class CachingService
        {
            private readonly IExpensiveService _inner;
            private readonly Dictionary<string, string> _cache;

            public CachingService(IExpensiveService inner)
            {
                _inner = inner;
                _cache = new Dictionary<string, string>();
            }

            public async ValueTask<string> GetAsync(string key)
            {
                if (_cache.TryGetValue(key, out var cachedValue))
                {
                    return new ValueTask<string>(cachedValue);
                }

                var result = await _inner.ComputeAsync(key);
                _cache[key] = result;
                return result;
            }
        }

if (_cache.TryGetValue(key, out var cachedValue))
        {
            return new System.Threading.Tasks.ValueTask<string>(cachedValue);
        }
        var result = await _inner.ComputeAsync(key);
        _cache[key] = result;
        return result;

...