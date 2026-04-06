class ThrottledProcessor
{
    private readonly SemaphoreSlim _semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        _semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public async Task<List<T>> ProcessAllAsync<T>(IEnumerable<Func<CancellationToken, Task<T>>> tasks, CancellationToken ct)
    {
        var results = new List<T>();
        var taskFactories = tasks.ToList();

        var tasksStarted = new List<Task<T>>();

        foreach (var taskFactory in taskFactories)
        {
            await _semaphore.WaitAsync(ct);
            try
            {
                var task = taskFactory(ct);
                tasksStarted.Add(task);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        var completedTasks = await Task.WhenAll(tasksStarted);

        results.AddRange(completedTasks);

        return results;
    }
}