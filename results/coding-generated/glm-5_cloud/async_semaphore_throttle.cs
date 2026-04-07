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

    public async Task<List> ProcessAllAsync(IEnumerable<Func<CancellationToken, Task>> tasks, CancellationToken ct)
    {
        var launchedTasks = new List<Task>();

        foreach (var taskFactory in tasks)
        {
            launchedTasks.Add(ProcessOneAsync(taskFactory, ct));
        }

        var results = await Task.WhenAll(launchedTasks);
        return new List(results);
    }

    private async Task ProcessOneAsync(Func<CancellationToken, Task> taskFactory, CancellationToken ct)
    {
        await _semaphore.WaitAsync(ct);
        try
        {
            await taskFactory(ct);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}