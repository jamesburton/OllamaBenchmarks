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
        var results = new List<T>();

        foreach (var taskFactory in tasks)
        {
            await _semaphore.WaitAsync(ct);
            var task = taskFactory(ct).ContinueWith(t =>
            {
                _semaphore.Release();
                return t;
            }, ct, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            taskList.Add(task.Unwrap());
        }

        var awaitedResults = await Task.WhenAll(taskList);
        return awaitedResults.ToList();
    }
}