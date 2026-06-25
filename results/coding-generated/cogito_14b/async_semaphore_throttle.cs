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
            await _semaphore.WaitAsync(ct).ConfigureAwait(false);

            try
            {
                taskList.Add(taskFactory(ct));
            }
            catch
            {
                _semaphore.Release();
                throw;
            }
        }

        var results = new List<T>(taskList.Count);

        foreach (var result in await Task.WhenAll(taskList))
        {
            results.Add(result);
        }

        return results;
    }
}