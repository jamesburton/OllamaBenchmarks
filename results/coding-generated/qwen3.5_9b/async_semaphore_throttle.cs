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
        var taskList = new List<Task<T>>(tasks.Count());
        var results = new List<T>(tasks.Count());

        foreach (var taskFactory in tasks)
        {
            await _semaphore.WaitAsync(ct);
            try
            {
                Task<T> task = taskFactory(ct);
                taskList.Add(task);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        var completedTasks = await Task.WhenAll(taskList.ToArray());
        results.AddRange(completedTasks);

        return results;
    }
}