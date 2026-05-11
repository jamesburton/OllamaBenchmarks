class ThrottledProcessor
{
    private readonly SemaphoreSlim _semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        _semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public Task<List<T>> ProcessAllAsync<T>(IEnumerable<Func<CancellationToken, Task<T>>> tasks, CancellationToken ct)
    {
        var processingTasks = new List<Task<T>>();

        foreach (var taskFactory in tasks)
        {
            // Create a task that encapsulates the throttling logic for this specific factory
            var workTask = Task.Run(async () =>
            {
                // Acquire the semaphore asynchronously
                await _semaphore.WaitAsync(ct);
                try
                {
                    // Execute the actual work defined by the factory
                    return await taskFactory(ct);
                }
                finally
                {
                    // Release the semaphore
                    _semaphore.Release();
                }
            });
            processingTasks.Add(workTask);
        }

        // Wait for all tasks to complete concurrently
        return Task.WhenAll(processingTasks).ContinueWith(_ => processingTasks.Select(t => t.Result).ToList());
    }
}