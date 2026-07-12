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
        var taskList = new List<Task<T>>();
        foreach (var taskFactory in tasks)
        {
            async Task<T> wrappedTask()
            {
                await _semaphore.WaitAsync(ct).ConfigureAwait(false);
                try
                {
                    return await taskFactory(ct).ConfigureAwait(false);
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            taskList.Add(wrappedTask());
        }

        var results = await Task.WhenAll(taskList).ConfigureAwait(false);
        return new List<T>(results);
    }
}