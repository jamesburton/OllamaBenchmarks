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
        var runningTasks = new Task[taskList.Count];

        for (int i = 0; i < taskList.Count; i++)
        {
            int index = i;
            runningTasks[index] = ExecuteAsync(index);
        }

        await Task.WhenAll(runningTasks);
        return new List<T>(results);

        async Task ExecuteAsync(int idx)
        {
            await _semaphore.WaitAsync(ct);
            try
            {
                results[idx] = await taskList[idx](ct);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}