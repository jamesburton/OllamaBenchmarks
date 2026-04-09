public class ThrottledProcessor
{
    private readonly System.Threading.SemaphoreSlim _semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        _semaphore = new System.Threading.SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public async System.Threading.Tasks.Task<System.Collections.Generic.List<T>> ProcessAllAsync<T>(System.Collections.Generic.IEnumerable<System.Func<System.Threading.CancellationToken, System.Threading.Tasks.Task<T>>> tasks, System.Threading.CancellationToken ct)
    {
        var taskList = new System.Collections.Generic.List<System.Threading.Tasks.Task<T>>();

        foreach (var taskFactory in tasks)
        {
            taskList.Add(ExecuteWithSemaphoreAsync(taskFactory, ct));
        }

        T[] results = await System.Threading.Tasks.Task.WhenAll(taskList);
        return new System.Collections.Generic.List<T>(results);
    }

    private async System.Threading.Tasks.Task<T> ExecuteWithSemaphoreAsync<T>(System.Func<System.Threading.CancellationToken, System.Threading.Tasks.Task<T>> taskFactory, System.Threading.CancellationToken ct)
    {
        await _semaphore.WaitAsync(ct);
        try
        {
            return await taskFactory(ct);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}