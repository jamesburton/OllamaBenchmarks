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
            try
            {
                var task = taskFactory(ct);
                taskList.Add(task);
            }
            catch
            {
                _semaphore.Release();
                throw;
            }
        }

        var completedTasks = await Task.WhenAll(taskList);
        results.AddRange(completedTasks);

        return results;
    }
}