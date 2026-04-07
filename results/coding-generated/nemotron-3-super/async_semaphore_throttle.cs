class ThrottledProcessor
{
    private readonly System.Threading.SemaphoreSlim _semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        _semaphore = new System.Threading.SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public System.Threading.Tasks.Task<System.Collections.Generic.List<T>> ProcessAllAsync<T>(
        System.Collections.Generic.IEnumerable<System.Func<System.Threading.CancellationToken, System.Threading.Tasks.Task<T>>> tasks,
        System.Threading.CancellationToken ct)
    {
        var tasksList = new System.Collections.Generic.List<System.Threading.Tasks.Task<T>>();

        foreach (var factory in tasks)
        {
            var t = System.Threading.Tasks.Task.Run(async () =>
            {
                await _semaphore.WaitAsync(ct).ConfigureAwait(false);
                try
                {
                    return await factory(ct).ConfigureAwait(false);
                }
                finally
                {
                    _semaphore.Release();
                }
            }, ct);
            tasksList.Add(t);
        }

        return System.Threading.Tasks.Task.WhenAll(tasksList)
            .ContinueWith(t => new System.Collections.Generic.List<T>(t.Result), System.Threading.Tasks.TaskContinuationOptions.ExecuteSynchronously);
    }
}