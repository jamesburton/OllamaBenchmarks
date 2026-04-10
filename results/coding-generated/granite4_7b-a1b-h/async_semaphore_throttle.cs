class ThrottledProcessor
{
    private readonly SemaphoreSlim _semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        _semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public async Task<List<T>> ProcessAllAsync<T>(IEnumerable<Func<CancellationToken, Task<T>>> tasks, CancellationToken ct)
    {
        var results = new List<T>(tasks.Count());
        var tasksToRun = new List<Task>();

        foreach (var taskFactory in tasks)
        {
            await _semaphore.WaitAsync(ct);
            tasksToRun.Add(Task.Run(async () =>
            {
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

        await Task.WhenAll(tasksToRun);
        for (int i = 0; i < tasks.Count(); i++)
        {
            results.Add(await tasksToRun[i]);
        }

        return results;
    }
}