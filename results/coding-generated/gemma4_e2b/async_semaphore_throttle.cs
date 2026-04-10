public class ThrottledProcessor
{
    private readonly SemaphoreSlim _semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        _semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public async Task<List<T>> ProcessAllAsync<T>(IEnumerable<Func<CancellationToken, Task<T>>> tasks, CancellationToken ct)
    {
        var taskFactories = new List<Func<CancellationToken, Task<T>>>();
        foreach (var taskFactory in tasks)
        {
            taskFactories.Add(taskFactory);
        }

        var processingTasks = new List<Task<T>>();
        var results = new List<T>(taskFactories.Count());

        for (int i = 0; i < taskFactories.Count; i++)
        {
            var taskFactory = taskFactories[i];
            var task = Task.Run(async () =>
            {
                await _semaphore.WaitAsync(ct);
                try
                {
                    var result = await taskFactory(ct);
                    return (i, result);
                }
                finally
                {
                    _semaphore.Release();
                }
            });
            processingTasks.Add(task);
        }

        var resultsArray = await Task.WhenAll(processingTasks);

        // Reorder results based on the original index
        for (int i = 0; i < taskFactories.Count; i++)
        {
            results.Add(resultsArray[i].Item2);
        }

        return results;
    }
}