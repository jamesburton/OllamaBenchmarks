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
            // Create a task that handles the throttling logic
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
                    // Ensure release happens even if the task fails
                    _semaphore.Release();
                }
            };

            // Start the task and add it to the list
            taskList.Add(throttledTask());
        }

        // Wait for all tasks to complete concurrently
        var resultsArray = await Task.WhenAll(taskList);

        // Convert the array of results to a List<T>
        return new List<T>(resultsArray);
    }
}