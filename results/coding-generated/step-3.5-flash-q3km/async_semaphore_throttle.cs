public class ThrottledProcessor
{
    private readonly System.Threading.SemaphoreSlim _semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        _semaphore = new System.Threading.SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public async System.Threading.Tasks.Task<System.Collections.Generic.List<T>> ProcessAllAsync<T>(
        System.Collections.Generic.IEnumerable<System.Func<System.Threading.CancellationToken, System.Threading.Tasks.Task<T>>> tasks, 
        System.Threading.CancellationToken ct)
    {
        var taskFactories = new System.Collections.Generic.List<System.Func<System.Threading.CancellationToken, System.Threading.Tasks.Task<T>>>(tasks);
        var wrappedTasks = new System.Collections.Generic.List<System.Threading.Tasks.Task<T>>(taskFactories.Count);

        foreach (var taskFactory in taskFactories)
        {
            wrappedTasks.Add(WrapTask(taskFactory, ct));
        }

        var results = await System.Threading.Tasks.Task.WhenAll(wrappedTasks).ConfigureAwait(false);
        return new System.Collections.Generic.List<T>(results);
    }

    private async System.Threading.Tasks.Task<T> WrapTask<T>(
        System.Func<System.Threading.CancellationToken, System.Threading.Tasks.Task<T>> taskFactory, 
        System.Threading.CancellationToken ct)
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