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
            var task = taskFactory(ct);
            taskList.Add(task);
        }

        var allTasks = await Task.WhenAll(taskList);
        results.AddRange(allTasks);
        return results;
    }
}