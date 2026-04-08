public class ThrottledProcessor
{
    private System.Threading.SemaphoreSlim _semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        _semaphore = new System.Threading.SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public async System.Threading.Tasks.Task<System.Collections.Generic.List<T>> ProcessAllAsync<T>(
        System.Collections.Generic.IEnumerable<System.Func<System.Threading.CancellationToken, System.Threading.Tasks.Task<T>>> tasks,
        System.Threading.CancellationToken ct)
    {
        var taskList = new System.Collections.Generic.List<System.Func<System.Threading.CancellationToken, System.Threading.Tasks.Task<T>>>(tasks);
        var taskCount = taskList.Count;
        var runningTasks = new System.Threading.Tasks.Task<T>[taskCount];

        for (var i = 0; i < taskCount; i++)
        {
            var factory = taskList[i];
            runningTasks[i] = Task.Run(async () =>
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
            });
        }

        var results = await System.Threading.Tasks.Task.WhenAll(runningTasks).ConfigureAwait(false);
        return new System.Collections.Generic.List<T>(results);
    }
}