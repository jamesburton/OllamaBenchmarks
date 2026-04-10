public class ThrottledProcessor
{
    private readonly SemaphoreSlim _semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        _semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public async Task<List<T>> ProcessAllAsync<T>(IEnumerable<Func<CancellationToken, Task<T>>> tasks, CancellationToken ct)
    {
        var taskFactories = tasks.ToList();
        var taskList = new Task<T>[taskFactories.Count];

        for (int i = 0; i < taskFactories.Count; i++)
        {
            taskList[i] = ProcessSingleAsync(taskFactories[i], i, ct);
        }

        await Task.WhenAll(taskList).ConfigureAwait(false);

        return taskList.Select(t => t.Result).ToList();
    }

    private async Task<T> ProcessSingleAsync<T>(Func<CancellationToken, Task<T>> taskFactory, int index, CancellationToken ct)
    {
        await _semaphore.WaitAsync(ct).ConfigureAwait(false);

        try
        {
            return await taskFactory(ct).ConfigureAwait(false);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}