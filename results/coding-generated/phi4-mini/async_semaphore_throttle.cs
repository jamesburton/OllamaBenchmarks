using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class ThrottledProcessor<T>
{
    private readonly SemaphoreSlim _semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        _semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public async Task<List<T>> ProcessAllAsync<T>(IEnumerable<Func<CancellationToken, Task<T>>> tasks, CancellationToken ct)
    {
        var taskFactories = tasks.Select(taskFactory => () =>
            await _semaphore.WaitAsync(ct).ConfigureAwait(false),
                   task: (await taskFactory(ct)).ContinueWith(
                       (_, cancellationToken) => _semaphore.Release(),
                       ct,
                       TaskContinuationOptions.ExecuteSynchronously));

        return new List<T>(taskFactories)
               .OrderBy(t => tasks.IndexOf(taskFactory))
               .SelectMany(t => t.Result).ToList();
    }
}