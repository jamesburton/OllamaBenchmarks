using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

class ThrottledProcessor
{
    private readonly SemaphoreSlim semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public async Task<List<T>> ProcessAllAsync<T>(IEnumerable<Func<CancellationToken, Task<T>>> tasks, CancellationToken ct)
    {
        var taskFactories = new List<Func<CancellationToken, Task<T>>>(tasks);
        var tasksToRun = new List<Task<T>>(taskFactories.Count);

        foreach (var taskFactory in taskFactories)
        {
            var task = Task.Run(async () =>
            {
                await semaphore.WaitAsync(ct);
                try
                {
                    return await taskFactory(ct);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            tasksToRun.Add(task);
        }

        var results = await Task.WhenAll(tasksToRun);
        return new List<T>(results);
    }
}