public class ThrottledProcessor
{
    private readonly SemaphoreSlim _semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        _semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public async Task<List<T>> ProcessAllAsync<T>(IEnumerable<Func<CancellationToken, Task<T>>> tasks, CancellationToken ct)
    {
        var taskFactories = new List<Func<CancellationToken, Task<T>>>(tasks);
        var processingTasks = new List<Task<T>>(taskFactories.Count);
        var results = new List<T>(taskFactories.Count);

        for (int i = 0; i < taskFactories.Count; i++)
        {
            var taskFactory = taskFactories[i];

            // Create a task that handles acquiring the semaphore, running the work, and releasing the semaphore
            var processingTask = Task.Run(async () =>
            {
                await _semaphore.WaitAsync(ct);
                try
                {
                    // Execute the actual work
                    var result = await taskFactory(ct);
                    results.Add(result);
                }
                finally
                {
                    _semaphore.Release();
                }
            }, ct);

            processingTasks.Add(processingTask);
        }

        // Wait for all the wrapper tasks to complete
        await Task.WhenAll(processingTasks);

        // Since we added results in the order of iteration, the list is correctly ordered.
        return results;
    }
}