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
            await _semaphore.WaitAsync(ct);
            try
            {
                taskList.Add(taskFactory(ct));
            }
            finally
            {
                _semaphore.Release();
            }
        }
        return (await Task.WhenAll(taskList)).ToList();
    }
}