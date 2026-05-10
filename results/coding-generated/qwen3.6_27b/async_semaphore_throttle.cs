class ThrottledProcessor
{
    private readonly SemaphoreSlim _semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        _semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public async Task<List<T>> ProcessAllAsync<T>(IEnumerable<Func<CancellationToken, Task<T>>> tasks, CancellationToken ct)
    {
        var taskFactories = tasks.ToList();
        var results = new T[taskFactories.Count];
        var tasksToAwait = new Task[taskFactories.Count];

        for (int i = 0; i < taskFactories.Count; i++)
        {
            int index = i;
            var factory = taskFactories[i];
            tasksToAwait[i] = Task.Run(async () =>
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
            }, ct);
        }

        await Task.WhenAll(tasksToAwait);

        for (int i = 0; i < tasksToAwait.Length; i++)
        {
            results[i] = ((Task<T>)tasksToAwait[i]).Result;
        }

        return results.ToList();
    }
}