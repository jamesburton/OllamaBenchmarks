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
        var tasksToRun = new List<Task<T>>(taskFactories.Count);
        var results = new List<T>(taskFactories.Count);

        for (int i = 0; i < taskFactories.Count; i++)
        {
            int index = i;
            tasksToRun.Add(Task.Run(async () =>
            {
                await _semaphore.WaitAsync(ct);
                try
                {
                    return await taskFactories[index](ct);
                }
                finally
                {
                    _semaphore.Release();
                }
            }));
        }

        results.AddRange(await Task.WhenAll(tasksToRun));
        return results;
    }
}