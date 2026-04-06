class ThrottledProcessor
{
    private readonly SemaphoreSlim semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public async Task<List<T>> ProcessAllAsync<T>(IEnumerable<Func<CancellationToken, Task<T>>> tasks, CancellationToken ct)
    {
        var tasksList = new List<Task<T>>();
        foreach (var factory in tasks)
        {
            tasksList.Add(Task.Run(async () =>
            {
                await semaphore.WaitAsync(ct);
                try
                {
                    return await factory(ct);
                }
                finally
                {
                    semaphore.Release();
                }
            }, ct));
        }

        var results = await Task.WhenAll(tasksList);
        return results.ToList();
    }
}