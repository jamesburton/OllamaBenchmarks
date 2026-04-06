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
            var task = Task.Run(async () =>
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
            taskList.Add(task);
        }

        await Task.WhenAll(taskList);

        return taskList.Select(t => t.Result).ToList();
    }
}