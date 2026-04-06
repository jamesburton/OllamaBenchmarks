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

    public async Task<List<T>> ProcessAllAsync<T>(
        IEnumerable<Func<CancellationToken, Task<T>>> tasks,
        CancellationToken ct)
    {
        var taskFactories = tasks.ToList();
        var results = new List<T>(taskFactories.Count);

        var taskList = new List<Task<T>>(taskFactories.Count);

        foreach (var factory in taskFactories)
        {
            taskList.Add(Task.Run(async () =>
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
            }, ct));
        }

        var resultsTask = Task.WhenAll(taskList);
        await resultsTask;

        results.AddRange(taskList.Select(t => t.Result));
        return results;
    }
}