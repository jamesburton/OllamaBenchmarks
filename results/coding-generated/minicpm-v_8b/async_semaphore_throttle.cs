using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ThrottledProcessor<T>
{
    private readonly SemaphoreSlim _semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        if (maxConcurrent <= 0) throw new ArgumentException("Max concurrent must be greater than zero.");
        _semaphore = new SemaphoreSlim(maxConcurrent);
    }

    public async Task<List<T>> ProcessAllAsync(IEnumerable<Func<CancellationToken, Task<T>>> tasks, CancellationToken ct)
    {
        var results = new List<T>();
        foreach (var taskFactory in tasks)
        {
            await _semaphore.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                results.Add(await taskFactory(ct));
            }
            finally
            {
                _semaphore.Release();
            }
        }

        return results;
    }
}