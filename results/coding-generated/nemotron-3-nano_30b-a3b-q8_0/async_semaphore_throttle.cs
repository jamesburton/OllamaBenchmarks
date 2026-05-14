public class ThrottledProcessor
{
    private readonly SemaphoreSlim _semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        _semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public async Task<List<T>> ProcessAllAsync<T>(IEnumerable<Func<CancellationToken, Task<T>>> tasks, CancellationToken ct)
    {
        var tasksList = tasks.ToList();

        var waitTasks = new List<Task>();
        foreach (var taskFactory in tasksList)
        {
            var waitTask = _semaphore.WaitAsync(ct);
            var workTask = Task.Run(async () =>
            {
                try
                {
                    await waitTask;
                    return await taskFactory(ct);
                }
                finally
                {
                    _semaphore.Release();
                }
            });
            waitTasks.Add(workTask);
        }

        var results = await Task.WhenAll(waitTasks);
        return results.ToList();
    }
}