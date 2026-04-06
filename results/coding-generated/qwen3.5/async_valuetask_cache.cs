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

            public ValueTask<string> GetAsync(string key)
            {
                if (_cache.TryGetValue(key, out var cachedValue))
                {
                    return new ValueTask<string>(cachedValue);
                }

                return inner.ComputeAsync(key); // Wait, need to store result
            }
        }

if (_cache.TryGetValue(key, out var cachedValue))
        {
            return new ValueTask<string>(cachedValue);
        }

        var result = await _inner.ComputeAsync(key);
        _cache[key] = result;
        return result;

public ValueTask<string> GetAsync(string key)
        {
            if (_cache.TryGetValue(key, out var cachedValue))
            {
                return new ValueTask<string>(cachedValue);
            }

            return _inner.ComputeAsync(key); // This returns Task<string>
            // But the signature is ValueTask<string>
            // Task<string> is implicitly convertible to ValueTask<string> in C# 7.3+?
            // No, Task<T> is not implicitly convertible to ValueTask<T>.
            // However, Task<T> implements IAsyncOperation<T> (in some contexts) or just Task.
            // ValueTask<T> has a constructor that takes Task<T>.
            // So: return new ValueTask<string>(_inner.ComputeAsync(key));
            // But we need to store the result in the cache.
            // If we return the task directly, we haven't stored it yet.
            // We need to await it to store it.
            // But if we await it, the method becomes async, returning Task<string>.
            // The prompt asks for ValueTask<string> GetAsync.
            // This is a specific constraint.
        }

var task = _inner.ComputeAsync(key);
        _cache[key] = await task; // This requires async method

public ValueTask<string> GetAsync(string key)
        {
            if (_cache.TryGetValue(key, out var cachedValue))
            {
                return new ValueTask<string>(cachedValue);
            }

            var task = _inner.ComputeAsync(key);
            // We need to store the result.
            // We can't await here without making the method async.
            // But if we make it async, return type is Task.
            // Is there a way to return ValueTask from async method? No.
        }

var task = _inner.ComputeAsync(key);
        return new ValueTask<string>(task); // Doesn't cache yet.

var tcs = new TaskCompletionSource<string>();
        _inner.ComputeAsync(key).ContinueWith(t => {
            _cache[key] = t.Result;
            tcs.TrySetResult(t.Result);
        });
        return tcs.Task; // This is Task<string>, not ValueTask.

public ValueTask<string> GetAsync(string key)
        {
            if (_cache.TryGetValue(key, out var cachedValue))
            {
                return new ValueTask<string>(cachedValue);
            }

            // For the miss path, we must await.
            // But we can't await in a non-async method.
            // So we must make it async.
            // But then return type is Task.
        }