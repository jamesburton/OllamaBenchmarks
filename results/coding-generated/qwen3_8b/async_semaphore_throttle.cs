public class ThrottledProcessor
{
    private readonly SemaphoreSlim _semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        _semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public async Task<List<T>> ProcessAllAsync<T>(IEnumerable<Func<CancellationToken, Task<T>>> tasks, CancellationToken ct)
    {
        var wrappedTasks = new List<Task<T>>();
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
            wrappedTasks.Add(task);
        }
        var results = await Task.WhenAll(wrappedTasks);
        return results.ToList();
    }
}