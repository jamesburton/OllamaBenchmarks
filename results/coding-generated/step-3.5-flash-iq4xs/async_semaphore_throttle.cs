public class ThrottledProcessor
{
    private System.Threading.SemaphoreSlim semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        semaphore = new System.Threading.SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public async System.Threading.Tasks.Task<System.Collections.Generic.List<T>> ProcessAllAsync<T>(
        System.Collections.Generic.IEnumerable<System.Func<System.Threading.CancellationToken, System.Threading.Tasks.Task<T>>> tasks,
        System.Threading.CancellationToken ct)
    {
        var taskList = new System.Collections.Generic.List<System.Threading.Tasks.Task<T>>();
        foreach (var factory in tasks)
        {
            taskList.Add(ProcessWithThrottleAsync(factory, ct));
        }

        var results = await System.Threading.Tasks.Task.WhenAll(taskList).ConfigureAwait(false);
        return new System.Collections.Generic.List<T>(results);
    }

    private async System.Threading.Tasks.Task<T> ProcessWithThrottleAsync<T>(
        System.Func<System.Threading.CancellationToken, System.Threading.Tasks.Task<T>> factory,
        System.Threading.CancellationToken ct)
    {
        await semaphore.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            return await factory(ct).ConfigureAwait(false);
        }
        finally
        {
            semaphore.Release();
        }
    }
}