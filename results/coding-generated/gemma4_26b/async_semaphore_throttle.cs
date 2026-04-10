using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class ThrottledProcessor
{
    private readonly SemaphoreSlim _semaphore;

    public ThrottlingProcessor(int maxConcurrent)
    {
        _semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public async Task<List<T>> ProcessAllAsync<T>(IEnumerable<Func<CancellationToken, Task<T>>> tasks, CancellationToken ct)
    {
        var taskList = tasks.ToList();
        var processingTasks = taskList.Select(async factory =>
        {
            await _semaphore.WaitAsync(ct);
            try
            {
                return await factory(ct);
            }
            finally
            {
                _semaphore.Release();
            }
        }).ToList();

        T[] results = await Task.WhenAll(processingTasks);
        return results.ToList();
    }
}