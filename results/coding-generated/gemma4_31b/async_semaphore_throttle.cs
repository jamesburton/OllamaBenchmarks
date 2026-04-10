public class ThrottledProcessor
{
    private readonly System.Threading.SemaphoreSlim _semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        _semaphore = new System.Threading.SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public async System.Threading.Tasks.Task<System.Collections.Generic.List<T>> ProcessAllAsync<T>(System.Collections.Generic.IEnumerable<System.Func<System.Threading.CancellationToken, System.Threading.Tasks.Task<T>>> tasks, System.Threading.CancellationToken ct)
    {
        async System.Threading.Tasks.Task<T> WrapAsync(System.Func<System.Threading.CancellationToken, System.Threading.Tasks.Task<T>> factory)
        {
            await _semaphore.WaitAsync(ct);
            try
            {
                return await factory(ct);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        var taskList = new System.Collections.Generic.List<System.Threading.Tasks.Task<T>>();
        foreach (var factory in tasks)
        {
            taskList.Add(WrapAsync(factory));
        }

        T[] results = await System.Threading.Tasks.Task.WhenAll(taskList);
        return new System.Collections.Generic.List<T>(results);
    }
}