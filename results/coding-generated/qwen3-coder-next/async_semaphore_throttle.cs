class ThrottledProcessor
{
    private readonly SemaphoreSlim _semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        _semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public async Task<List<T>> ProcessAllAsync<T>(IEnumerable<Func<CancellationToken, Task<T>>> tasks, CancellationToken ct)
    {
        if (tasks == null)
            return new List<T>();

        var taskList = tasks.ToList();
        var resultTasks = new List<Task<T>>();

        foreach (var taskFactory in taskList)
        {
            // Capture the current task factory in a local variable to avoid closure issues
            var factory = taskFactory;

            // Start a new task that will acquire semaphore, execute the factory, and release semaphore
            var resultTask = ExecuteWithSemaphoreAsync(factory, ct);
            resultTasks.Add(resultTask);
        }

        // Wait for all tasks to complete and get results in the same order
        return await Task.WhenAll(resultTasks).ConfigureAwait(false);
    }

    private async Task<T> ExecuteWithSemaphoreAsync<T>(Func<CancellationToken, Task<T>> taskFactory, CancellationToken ct)
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