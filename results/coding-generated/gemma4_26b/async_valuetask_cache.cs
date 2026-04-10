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
                if (_cache.TryGetValue(key, out var cachedValue))
                {
                    return new System.Threading.Tasks.ValueTask<string>(cachedValue);
                }

                return GetAsyncInternal(key);
            }

            private async System.Threading.Tasks.ValueTask<string> GetAsyncInternal(string key)
            {
                string result = await _inner.ComputeAsync(key);
                _cache[key] = result;
                return result;
            }
        }

public System.Threading.Tasks.ValueTask<string> GetAsync(string key)
        {
            if (_cache.TryGetValue(key, out var cachedValue))
            {
                return new System.Threading.Tasks.ValueTask<string>(cachedValue);
            }
            return GetAsyncInternal(key);
        }

        private async System.Threading.Tasks.ValueTask<string> GetAsyncInternal(string key)
        {
            var result = await _inner.ComputeAsync(key);
            _cache[key] = result;
            return result;
        }

public System.Threading.Tasks.ValueTask<string> GetAsync(string key)
        {
            if (_cache.TryGetValue(key, out var cachedValue))
            {
                return new System.Threading.Tasks.ValueTask<string>(cachedValue);
            }

            return GetAsyncInternal(key);
        }

        private async System.Threading.Tasks.ValueTask<string> GetAsyncInternal(string key)
        {
            string result = await _inner.ComputeAsync(key);
            _cache[key] = result;
            return result;
        }

public interface IExpensiveService
    {
        System.Threading.Tasks.Task<string> ComputeAsync(string key);
    }

    public class CachingService
    {
        private readonly IExpensive/ExpensiveService _inner;
        private readonly System.Collections.Generic.Dictionary<string, string> _cache = new System.Collections.Generic.Dictionary<string, string>();

        public CachingService(IExpensiveService inner)
        {
            _inner = inner;
        }

        public async System.Threading.Tasks.ValueTask<string> GetAsync(string key)
        {
            if (_cache.TryGetValue(key, out var cachedValue))
            {
                return new System.Threading.Tasks.ValueTask<string>(cachedValue);
            }

            string result = await _inner.ComputeAsync(key);
            _cache[key] = result;
            return result;
        }
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

        public async System.Threading.Tasks.ValueTask<string> GetAsync(string key)
        {
            if (_cache.TryGetValue(key, out var cachedValue))
            {
                return new System.Threading.Tasks.ValueTask<string>(cachedValue);
            }

            string result = await _inner.ComputeAsync(key);
            _cache[key] = result;
            return result;
        }
    }

public interface IExpensiveService
    ---------------------------------------------------------
    public class CachingService
    ---------------------------------------------------------