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
        var results = new List<T>();

        foreach (var taskFactory in tasks)
        {
            await _semaphore.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                var task = taskFactory(ct);
                taskList.Add(task);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        var completedTasks = await Task.WhenAll(taskList).ConfigureAwait(false);
        results.AddRange(completedTasks);

        return results;
    }
}