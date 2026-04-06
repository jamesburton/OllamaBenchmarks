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
            var task = Task.Run(async () =>
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
            });
            taskList.Add(task);
        }

        var taskResults = await Task.WhenAll(taskList);

        foreach (var result in taskResults)
        {
            results.Add(result);
        }

        return results;
    }
}