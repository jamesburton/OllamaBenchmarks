public class ThrottledProcessor
{
    private readonly SemaphoreSlim _semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        _semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public async Task<List<T>> ProcessAllAsync<T>(IEnumerable<Func<CancellationToken, Task<T>>> tasks, CancellationToken ct)
    {
        var taskFactories = tasks.ToList();
        var runningTasks = new Task<T>[taskFactories.Count];

        for (int i = 0; i < taskFactories.Count; i++)
        {
            runningTasks[i] = ExecuteThrottledAsync(taskFactories[i], ct);
        }

        T[] results = await Task.WhenAll(runningTasks);
        return results.ToList();
    }

    private async Task<T> ExecuteThrottledAsync<T>(Func<CancellationToken, Task<T>> taskFactory, CancellationToken ct)
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
    }
}