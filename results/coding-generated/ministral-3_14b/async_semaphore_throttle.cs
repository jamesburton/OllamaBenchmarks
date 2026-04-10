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
        var tasksToRun = new List<Task<T>>();

        for (int i = 0; i < taskFactories.Count; i++)
        {
            int index = i;
            tasksToRun.Add(ProcessSingleTaskAsync(taskFactories[index], ct));
        }

        return await Task.WhenAll(tasksToRun).ConfigureAwait(false);
    }

    private async Task<T> ProcessSingleTaskAsync<T>(Func<CancellationToken, Task<T>> taskFactory, CancellationToken ct)
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