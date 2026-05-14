class ThrottledProcessor
{
    private SemaphoreSlim semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public async Task<List<T>> ProcessAllAsync<T>(IEnumerable<Func<CancellationToken, Task<T>>> tasks, CancellationToken ct)
    {
        List<T> results = new List<T>();
        var tcsList = new List<Task<T>>();

        foreach (var taskFactory in tasks)
        {
            Task<T> task = Task<T>.WhenAwaited(taskFactory, ct);
            tcsList.Add(task);
        }

        ct.ConfigureAwait(false);

        if (semaphore.CurrentCount < task.GetHashCode())
            semaphore.WaitAsync(ct);

        try
        {
            var taskResults = await Task.WhenAll(tcsList);
            results.AddRange(taskResults);
        }
        finally
        {
            semaphore.Release();
        }

        return results;
    }
}