using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class ThrottledProcessor
{
    private readonly SemaphoreSlim semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public async Task<List<T>> ProcessAllAsync<T>(IEnumerable<Func<CancellationToken, Task<T>>> tasks, CancellationToken ct)
    {
        var taskList = new List<Task<T>>();
        foreach (var taskFactory in tasks)
        {
            await semaphore.WaitAsync(ct);
            try
            {
                var task = taskFactory(ct).ContinueWith(t => t.Result, ct);
                taskList.Add(task);
            }
            finally
            {
                semaphore.Release();
            }
        }

        var results = new List<T>();
        foreach (var task in await Task.WhenAll(taskList))
        {
            results.Add(task);
        }

        return results;
    }
}