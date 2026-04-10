public class ThrottledProcessor
{
    private readonly SemaphoreSlim _semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        _semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public async System.Threading.Tasks.Task<System.Collections.Generic.List<T>> ProcessAllAsync<T>(
        System.Collections.Generic.IEnumerable<System.Func<System.Threading.CancellationToken, System.Threading.Tasks.Task<T>>> tasks,
        System.Threading.CancellationToken ct)
    {
        var taskFactories = new System.Collections.Generic.List<System.Func<System.Threading.CancellationToken, System.Threading.Tasks.Task<T>>>(tasks);
        var results = new T[taskFactories.Count];
        var processingTasks = new System.Threading.Tasks.Task[taskFactories.Count];

        for (int i = 0; i < taskFactories.Count; i++)
        {
            int index = i;
            var factory = taskFactories[i];

            processingTasks[i] = System.Threading.Tasks.Task.Run(async () =>
            {
                await _semaphore.WaitAsync(ct).ConfigureAwait(false);
                try
                {
                    results[index] = await factory(ct).ConfigureAwait(false);
                }
                finally
                {
                    _semaphore.Release();
                }
            }, ct);
        }

        await System.Threading.Tasks.Task.WhenAll(processingTasks).ConfigureAwait(false);

        return new System.Collections.Generic.List<T>(results);
    }
}