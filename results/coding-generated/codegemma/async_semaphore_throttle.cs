public class ThrottledProcessor
{
    private readonly SemaphoreSlim _semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        _semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public async Task<List<T>> ProcessAllAsync<T>(IEnumerable<Func<CancellationToken, Task<T>>> tasks, CancellationToken ct)
    {
        var results = new List<T>();
        var tasksToRun = tasks.Select(task =>
        {
            return _semaphore.WaitAsync(ct).ContinueWith(async _ =>
            {
                try
                {
                    results.Add(await task(ct));
                }
                finally
                {
                    _semaphore.Release();
                }
            });
        });

        await Task.WhenAll(tasksToRun);
        return results;
    }
}