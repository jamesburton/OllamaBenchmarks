public class ThrottledProcessor
{
    private readonly SemaphoreSlim _semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        _semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public async Task<List<T>> ProcessAllAsync<T>(IEnumerable<Func<CancellationToken, Task<T>>> tasks, CancellationToken ct)
    {
        var taskList = tasks.Select(async factory =>
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
        });
        var results = await Task.WhenAll(taskList);
        return results.ToList();
    }
}