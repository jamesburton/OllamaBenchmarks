public class ThrottledProcessor
{
    private readonly SemaphoreSlim _semaphore;

    public ThrottledProcessor(int maxConcurrent, int initialCount = 1)
    {
        _semaphore = new SemaphoreSlim(initialCount, maxConcurrent);
    }

    public async Task<List<T>> ProcessAllAsync<T>(IEnumerable<Func<CancellationToken, Task<T>>> tasks, CancellationToken ct)
    {
        var results = new List<T>[tasks.Count()];
        var taskWrappers = new List<Task>();

        for (int i = 0; i < tasks.Count(); i++)
        {
            int index = i;
            var taskFactory = tasks.ElementAt(index);
            taskWrappers.Add(ProcessTaskAsync(taskFactory, ct, index, results, index));
        }

        await Task.WhenAll(taskWrappers);
        return results.ToList();
    }

    private async Task ProcessTaskAsync<T>(
        Func<CancellationToken, Task<T>> taskFactory,
        CancellationToken ct,
        int index,
        List<T> results, int resultIndex)
    {
        await _semaphore.WaitAsync(ct);
        try
        {
            results[index] = await taskFactory(ct);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}