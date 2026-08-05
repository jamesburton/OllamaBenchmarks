public class ThrottledProcessor
{
    private readonly SemaphoreSlim _semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        _semaphore = new SemaphoreSlim(maxConcurrent);
    }

    public async Task<List<T>> ProcessAllAsync<T>(IEnumerable<Func<CancellationToken, Task<T>>> tasks, CancellationToken ct)
    {
        var tasksList = new List<Task<T>>();
        var results = new List<T>();

        foreach (var taskFactory in tasks)
        {
            var task = Task.Run(async () =>
            {
                try
                {
                    await _semaphore.WaitAsync(ct).ConfigureAwait(false);
                    results.Add(await taskFactory(ct).ConfigureAwait(false));
                }
                finally
                {
                    _semaphore.Release();
                }
            }, ct);

            tasksList.Add(task);
        }

        await Task.WhenAll(tasksList).ConfigureAwait(false);
        return results;
    }
}