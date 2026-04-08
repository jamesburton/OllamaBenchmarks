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
        var taskList = new System.Collections.Generic.List<System.Threading.Tasks.Task<T>>();
        foreach (var taskFactory in tasks)
        {
            var currentTaskFactory = taskFactory;
            var task = System.Threading.Tasks.Task.Run(async () =>
            {
                await _semaphore.WaitAsync(ct).ConfigureAwait(false);
                try
                {
                    return await currentTaskFactory(ct).ConfigureAwait(false);
                }
                finally
                {
                    _semaphore.Release();
                }
            }, ct);
            taskList.Add(task);
        }

        var results = await System.Threading.Tasks.Task.WhenAll(taskList).ConfigureAwait(false);
        return new System.Collections.Generic.List<T>(results);
    }
}