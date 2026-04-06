using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class ThrottledProcessor
{
    private readonly SemaphoreSlim _semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        _semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public async Task<List<T>> ProcessAllAsync<T>(IEnumerable<Func<CancellationToken, Task<T>>> tasks, CancellationToken ct)
    {
        var tasksWithSemaphore = tasks.Select(async task =>
        {
            await _semaphore.WaitAsync(ct);
            try
            {
                return await task(ct);
            }
            finally
            {
                _semaphore.Release();
            }
        });

        var results = await Task.WhenAll(tasksWithSemaphore);
        return results.ToList();
    }
}