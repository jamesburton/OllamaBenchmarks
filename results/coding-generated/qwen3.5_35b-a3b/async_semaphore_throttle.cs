public class ThrottledProcessor
        {
            private readonly SemaphoreSlim _semaphore;

            public ThrottledProcessor(int maxConcurrent)
            {
                _semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
            }

            public async Task<List<T>> ProcessAllAsync<T>(IEnumerable<Func<CancellationToken, Task<T>>> tasks, CancellationToken ct)
            {
                // Implementation needed here
            }
        }

var taskList = tasks.Select(t =>
        {
            return Task.Run(async () =>
            {
                await _semaphore.WaitAsync(ct);
                try
                {
                    return await t(ct);
                }
                finally
                {
                    _semaphore.Release();
                }
            });
        }).ToList();

        var results = await Task.WhenAll(taskList);
        return results.ToList();

async Task<T> Worker(Func<CancellationToken, Task<T>> func, CancellationToken ct)
        {
            await _semaphore.WaitAsync(ct);
            try
            {
                return await func(ct);
            }
            finally
            {
                _semaphore.Release();
            }
        }

var tasks = tasksInput.Select(t => Worker(t, ct));
        var results = await Task.WhenAll(tasks);