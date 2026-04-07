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
        // Materialize the factories to preserve order and allow indexing
        var factories = new System.Collections.Generic.List<System.Func<System.Threading.CancellationToken, System.Threading.Tasks.Task<T>>>(tasks);
        var results = new System.Threading.Tasks.Task<T>[factories.Count];

        // Local function that respects the semaphore
        async System.Threading.Tasks.Task<T> RunFactoryAsync(
            System.Func<System.Threading.CancellationToken, System.Threading.Tasks.Task<T>> factory,
            System.Threading.CancellationToken token)
        {
            await _semaphore.WaitAsync(token).ConfigureAwait(false);
            try
            {
                return await factory(token).ConfigureAwait(false);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        // Start all tasks (concurrency limited by the semaphore)
        for (int i = 0; i < factories.Count; i++)
        {
            results[i] = RunFactoryAsync(factories[i], ct);
        }

        // Await completion of all tasks
        await System.Threading.Tasks.Task.WhenAll(results).ConfigureAwait(false);

        // Collect results preserving the original order
        var orderedResults = new System.Collections.Generic.List<T>(results.Length);
        foreach (var task in results)
        {
            orderedResults.Add(task.Result);
        }

        return orderedResults;
    }
}