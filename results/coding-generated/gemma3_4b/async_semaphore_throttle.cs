public class ThrottledProcessor
{
    private readonly SemaphoreSlim _semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        _semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public async Task<List<T>> ProcessAllAsync<T>(IEnumerable<Func<CancellationToken, Task<T>>> tasks, CancellationToken ct)
    {
        List<Task<T>> tasksToStart = new List<Task<T>>();

        foreach (var taskFactory in tasks)
        {
            var task = taskFactory(ct);
            tasksToStart.Add(task);
        }

        await Task.WhenAll(tasksToStart);

        List<T> results = new List<T>();
        foreach (var task in tasksToStart)
        {
            try
            {
                results.Add(await task);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        return results;
    }
}