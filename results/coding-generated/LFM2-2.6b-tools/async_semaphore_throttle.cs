public class ThrottledProcessor

{
    private readonly SemaphoreSlim _semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        _semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public async Task<List<T>> ProcessAllAsync<T>(IEnumerable<Func<CancellationToken, Task<T>>> tasks, CancellationToken ct)
    {
        var taskTasks = tasks.Select(t => Task.Run(async () =>
            await _semaphore.WaitAsync(ct)
            using (_semaphore) // Note: SemaphoreSlim doesn't support using, but we release in finally block
            {
                try
                {
                    await t(ct);
                }
                finally
                {
                    _semaphore.Release();
                }
            })).ToList();

        await Task.WhenAll(taskTasks);

        var results = new List<T>(tasks.Count());
        for (int i = 0; i < tasks.Count(); i++)
        {
            results[i] = taskTasks[i].Result;
        }
        return results;
    }
}