class ThrottledProcessor
{
    private readonly SemaphoreSlim semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public async Task<List<T>> ProcessAllAsync<T>(IEnumerable<Func<CancellationToken, Task<T>>> tasks, CancellationToken ct)
    {
        var throttledTasks = new List<Task<T>>();

        foreach (var taskFactory in tasks)
        {
            throttledTasks.Add(Task.Run(async () =>
            {
                try
                {
                    await semaphore.WaitAsync(ct);
                    return await taskFactory(ct);
                }
                finally
                {
                    semaphore.Release();
                }
            }, ct));
        }

        var results = await Task.WhenAll(throttledTasks);
        return results.ToList();
    }
}