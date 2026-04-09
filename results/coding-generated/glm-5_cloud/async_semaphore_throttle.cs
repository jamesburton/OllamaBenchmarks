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

        foreach (var factory in tasks)
        {
            taskList.Add(ProcessItemAsync(factory, ct));
        }

        T[] results = await Task.WhenAll(taskList);
        return new List<T>(results);
    }

    private async Task<T> ProcessItemAsync<T>(Func<CancellationToken, Task<T>> factory, CancellationToken ct)
    {
        await _semaphore.WaitAsync(ct);
        try
        {
            return await factory(ct);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}