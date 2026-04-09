public class ThrottledProcessor
{
    private readonly SemaphoreSlim _semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        _semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public async Task<List<T>> ProcessAllAsync<T>(IEnumerable<Func<CancellationToken, Task<T>>> tasks, CancellationToken ct)
    {
        var taskList = new List<Func<CancellationToken, Task<T>>>(tasks);
        var results = new T[taskList.Count];
        var concurrentTasks = new List<Task>();

        for (int i = 0; i < taskList.Count; i++)
        {
            int index = i;
            var factory = taskList[i];
            concurrentTasks.Add(ExecuteAsync(factory, ct, index, results));
        }

        await Task.WhenAll(concurrentTasks);
        return new List<T>(results);
    }

    private async Task ExecuteAsync<T>(Func<CancellationToken, Task<T>> factory, CancellationToken ct, int index, T[] results)
    {
        await _semaphore.WaitAsync(ct);
        try
        {
            results[index] = await factory(ct);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}