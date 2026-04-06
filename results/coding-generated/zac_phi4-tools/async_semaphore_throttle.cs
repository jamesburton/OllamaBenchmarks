using System;
using System.Collections.Generic;
using System.Linq;
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
        var taskFactories = tasks.ToList();
        var results = new List<Task<T>>(taskFactories.Count);

        for (int i = 0; i < taskFactories.Count; i++)
        {
            int index = i;
            Func<CancellationToken, Task<T>> taskFactory = taskFactories[index];

            results.Add(Task.Run(async () =>
            {
                await _semaphore.WaitAsync(ct);
                try
                {
                    return await taskFactory(ct);
                }
                finally
                {
                    _semaphore.Release();
                }
            }, ct));
        }

        var completedTasks = await Task.WhenAll(results);

        return completedTasks.ToList();
    }
}