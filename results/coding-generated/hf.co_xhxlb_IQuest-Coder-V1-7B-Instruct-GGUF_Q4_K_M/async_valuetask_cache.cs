using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CachingExample
{
    // 1. Interface
    public interface IExpensiveService
    {
        Task<string> ComputeAsync(string key);
    }

    // 2. Implementation
    public class CachingService : IExpensiveService
    {
        private readonly IExpensiveService _inner;
        private readonly Dictionary<string, string> _cache;

        public CachingService(IExpensiveService inner)
        {
            _inner = inner;
            _cache = new Dictionary<string, string>();
        }

        public async Task<string> ComputeAsync(string key)
        {
            // 1. Check cache
            if (_cache.TryGetValue(key, out string cachedValue))
            {
                // 2. If hit, return cached value synchronously
                return new ValueTask<string>(cachedValue);
            }

            // 3. If miss, call inner service
            string result = await _inner.ComputeAsync(key);

            // 4. Store in cache and return
            _cache[key] = result;
            return new ValueTask<string>(result);
        }
    }
}