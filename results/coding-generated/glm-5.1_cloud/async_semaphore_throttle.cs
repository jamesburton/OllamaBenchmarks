public class ThrottledProcessor
{
    private readonly SemaphoreSlim _semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        _semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public async Task<List<T>> ProcessAllAsync<T>(IEnumerable<Func<CancellationToken, Task<T>>> tasks, CancellationToken ct)
    {
        var taskList = new List<Func<CancellationToken, Task<T>>>();
        foreach (var t in tasks)
        {
            taskList.Add(t);
        }

        var results = new T[taskList.Count];
        var concurrentTasks = new Task[taskList.Count];

        for (int i = 0; i < taskList.Count; i++)
        {
            int index = i;
            concurrentTasks[index] = ExecuteThrottledAsync(taskList[index], index, ct, results);
        }

        await Task.WhenAll(concurrentTasks);
        return new List<T>(results);
    }

    private async Task ExecuteThrottledAsync<T>(Func<CancellationToken, Task<T>> factory, int index, CancellationToken ct, T[] results)
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