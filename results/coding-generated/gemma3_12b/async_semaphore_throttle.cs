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
        List<Task<T>> runningTasks = new List<Task<T>>();
        List<T> results = new List<T>();

        foreach (var taskFactory in tasks)
        {
            await _semaphore.WaitAsync(ct);
            try
            {
                var task = taskFactory(ct);
                runningTasks.Add(task);
                // Start the task immediately and don't await here.
            }
            finally
            {
                // Ensure the semaphore is released even if the task factory throws an exception.
                // This is crucial to prevent deadlocks.
                _semaphore.Release();
            }
        }

        // Await all tasks concurrently.
        await Task.WhenAll(runningTasks);

        // Collect results in the original order.
        for (int i = 0; i < runningTasks.Count; i++)
        {
            results.Add(runningTasks[i].Result);
        }

        return results;
    }
}