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
            // Check cache
            if (_cache.TryGetValue(key, out string cachedValue))
            {
                // Return synchronously
                return cachedValue;
            }

            // Miss path
            string result = await _inner.ComputeAsync(key);
            _cache[key] = result;
            return result;
        }
    }

public ValueTask<string> GetAsync(string key)
        {
            if (_cache.TryGetValue(key, out string value))
            {
                return new ValueTask<string>(value);
            }
            return ComputeAndCacheAsync(key);
        }

        private async Task<string> ComputeAndCacheAsync(string key)
        {
            var result = await _inner.ComputeAsync(key);
            _cache[key] = result;
            return result;
        }

public ValueTask<string> GetAsync(string key)
        {
            if (_cache.TryGetValue(key, out string cachedValue))
            {
                return new ValueTask<string>(cachedValue);
            }
            return FetchAsync(key);
        }

        private async ValueTask<string> FetchAsync(string key)
        {
            string result = await _inner.ComputeAsync(key);
            _cache[key] = result;
            return result;
        }

public async ValueTask<string> GetAsync(string key)
            {
                if (_cache.TryGetValue(key, out string cachedValue))
                {
                     // Prompt says use: return new ValueTask<string>(cachedValue);
                     // This is invalid syntax inside an async method.
                     // I would have to write: return cachedValue;
                }
                // ...
            }

public ValueTask<string> GetAsync(string key)
        {
            if (_cache.TryGetValue(key, out string cachedValue))
            {
                return new ValueTask<string>(cachedValue);
            }

            return Impl();

            async ValueTask<string> Impl()
            {
                var result = await _inner.ComputeAsync(key);
                _cache[key] = result;
                return result;
            }
        }

using System.Collections.Generic; // Wait, prompt says "Do NOT add using statements".
    using System.Threading.Tasks;     // Wait, prompt says "Do NOT add using statements".

public interface IExpensiveService
        {
            System.Threading.Tasks.Task<string> ComputeAsync(string key);
        }

public class CachingService
        {
            private readonly IExpensiveService _inner;
            private readonly System.Collections.Generic.Dictionary<string, string> _cache;

            public CachingService(IExpensiveService inner)
            {
                _inner = inner;
                _cache = new System.Collections.Generic.Dictionary<string, string>();
            }

            public System.Threading.Tasks.ValueTask<string> GetAsync(string key)
            {
                if (_cache.TryGetValue(key, out string cachedValue))
                {
                    return new System.Threading.Tasks.ValueTask<string>(cachedValue);
                }

                return GetAsyncCore(key);

                async System.Threading.Tasks.ValueTask<string> GetAsyncCore(string k)
                {
                    string result = await _inner.ComputeAsync(k);
                    _cache[k] = result;
                    return result;
                }
            }
        }

public async System.Threading.Tasks.ValueTask<string> GetAsync(string key)
        {
            if (_cache.TryGetValue(key, out string cachedValue))
            {
                // Prompt says: return new ValueTask<string>(cachedValue);
                // This line causes compiler error CS0029 Cannot implicitly convert type 'System.Threading.Tasks.ValueTask<string>' to 'string'
                return new System.Threading.Tasks.ValueTask<string>(cachedValue);
            }
            // ...
        }

public interface IExpensiveService
    {
        System.Threading.Tasks.Task<string> ComputeAsync(string key);
    }

    public class CachingService
    {
        private readonly IExpensiveService _inner;
        private readonly System.Collections.Generic.Dictionary<string, string> _cache = new System.Collections.Generic.Dictionary<string, string>();

        public CachingService(IExpensiveService inner)
        {
            _inner = inner;
        }

        public System.Threading.Tasks.ValueTask<string> GetAsync(string key)
        {
            if (_cache.TryGetValue(key, out string cachedValue))
            {
                return new System.Threading.Tasks.ValueTask<string>(cachedValue);
            }

            return ComputeAndCacheAsync(key);
        }

        private async System.Threading.Tasks.ValueTask<string> ComputeAndCacheAsync(string key)
        {
            string result = await _inner.ComputeAsync(key);
            _cache[key] = result;
            return result;
        }
    }

public System.Threading.Tasks.ValueTask<string> GetAsync(string key)
        {
            if (_cache.TryGetValue(key, out string cachedValue))
            {
                return new System.Threading.Tasks.ValueTask<string>(cachedValue);
            }
            // How to use async/await here without making method async?
            // I can't.
            // How to return ValueTask without new keyword if method is async?
            // return cachedValue; (Compiler wraps it).
        }