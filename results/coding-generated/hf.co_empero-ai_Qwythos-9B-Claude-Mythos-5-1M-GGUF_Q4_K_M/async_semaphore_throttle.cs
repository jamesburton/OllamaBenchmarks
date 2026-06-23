public class ThrottledProcessor
{
    private readonly SemaphoreSlim _semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        _semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public async Task<List<T>> ProcessAllAsync<T>(IEnumerable<Func<CancellationToken, Task<T>>> tasks, CancellationToken ct)
    {
        var results = new List<T>();
        foreach (var taskFactory in tasks)
        {
            await _semaphore.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                var result = await taskFactory(ct).ConfigureAwait(false);
                results.Add(result);
            }
            finally
            {
                _semaphore.Release();
            }
        }
        return results;
    }
}