using System;
using System.Threading;
using System.Threading.Tasks;

public class ThrottledProcessor<T>
{
    private SemaphoreSlim semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        semaphore = new SemaphoreSlim(maxConcurrent);
    }

    public Task<List<T>> ProcessAllAsync(IEnumerable<Func<CancellationToken, Task<T>>> tasks, CancellationToken ct)
    {
        Task<List<T>> result = Task.WhenAll(tasks.Select(t => semaphore.WaitAsync(ct).ContinueWith(t)));
        result.ContinueWith(t => semaphore.Release(), t => t);
        return result;
    }
}