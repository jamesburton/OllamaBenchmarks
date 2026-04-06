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
        var runningTasks = new System.Collections.Generic.List<System.Threading.Tasks.Task<T>>();

        foreach (var taskFactory in tasks)
        {
            var task = System.Threading.Tasks.Task.Run(async () =>
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
            }, ct);

            runningTasks.Add(task);
        }

        await System.Threading.Tasks.Task.WhenAll(runningTasks).ConfigureAwait(false);

        var results = new System.Collections.Generic.List<T>(runningTasks.Count);
        foreach (var t in runningTasks)
        {
            results.Add(t.Result);
        }

        return results;
    }
}