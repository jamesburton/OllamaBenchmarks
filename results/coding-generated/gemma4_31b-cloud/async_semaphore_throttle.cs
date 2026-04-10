public class ThrottledProcessor
{
    private readonly SemaphoreSlim _semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        _semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public async Task<List<T>> ProcessAllAsync<T>(IEnumerable<Func<CancellationToken, Task<T>>> tasks, CancellationToken ct)
    {
        var taskList = new List<Task<T>>();

        foreach (var taskFactory in tasks)
        {
            taskList.Add(ExecuteWithSemaphoreAsync(taskFactory, ct));
        }

        T[] results = await Task.WhenAll(taskList);
        return new List<T>(results);
    }

    private async Task<T> ExecuteWithSemaphoreAsync<T>(Func<CancellationToken, Task<T>> taskFactory, CancellationToken ct)
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