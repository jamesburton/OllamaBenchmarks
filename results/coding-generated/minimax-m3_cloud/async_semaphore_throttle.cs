public class ThrottledProcessor
{
    private readonly SemaphoreSlim _semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        _semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public async Task<List<T>> ProcessAllAsync<T>(IEnumerable<Func<CancellationToken, Task<T>>> tasks, CancellationToken ct)
    {
        var launched = new List<Task<T>>();

        foreach (var taskFactory in tasks)
        {
            launched.Add(RunWithThrottleAsync(taskFactory, ct));
        }

        var results = await Task.WhenAll(launched);
        return new List<T>(results);
    }

    private async Task<T> RunWithThrottleAsync<T>(Func<CancellationToken, Task<T>> taskFactory, CancellationToken ct)
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