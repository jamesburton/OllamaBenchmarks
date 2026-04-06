public class ThrottledProcessor
{
    private readonly SemaphoreSlim _semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        _semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public async Task<List<T>> ProcessAllAsync<T>(IEnumerable<Func<CancellationToken, Task<T>>> tasks, CancellationToken ct)
    {
        var executionTasks = new List<Task<T>>();

        foreach (var taskFactory in tasks)
        {
            // Create a task that handles the throttling logic (acquire/release)
            var throttledTask = async () =>
            {
                await _semaphore.WaitAsync(ct);
                try
                {
                    // Execute the actual work
                    return await taskFactory(ct);
                }
                finally
                {
                    // Ensure release happens regardless of success or failure
                    _semaphore.Release();
                }
            };

            // Start the task and add it to the list
            executionTasks.Add(throttledTask());
        }

        // Run all wrapped tasks concurrently
        var results = await Task.WhenAll(executionTasks);

        // Return the results as a List<T>
        return results.ToList();
    }
}