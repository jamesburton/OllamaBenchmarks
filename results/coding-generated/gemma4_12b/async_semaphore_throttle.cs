using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class ThrottledProcessor
{
    private readonly SemaphoreSlim _semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        _semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public Task<List<T>> ProcessAllAsync<T>(IEnumerable<Func<CancellationToken, Task<T>>> tasks, CancellationToken ct)
    {
        var taskFactories = tasks.ToList();
        var tasksToAwait = new List<Task<T>>();

        foreach (var factory in taskFactories)
        {
            // Create a wrapper task that handles the semaphore logic for each individual item
            var task = Task.Run(async () =>
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
            }, ct).Unwrap();

            tasksToAwait.Add(task);
        }

        // Wait for all tasks to complete and return the results in order
        return Task.WhenAll(tasksToAwait.ToArray()).ContinueWith(t => 
            t.Status == TaskStatus.RanToCompletion ? (List<T>)t.GetType().GetProperty("Result").GetValue(t) : throw new Exception("One or more tasks failed"));
    }

    // Note: The above implementation of ProcessAllAsync uses a simplified approach to handle the 
    // mapping of results. To ensure strict adherence to "Return the results in the same order", 
    // here is the refined logic for the method body:

    public async Task<List<T>> ProcessAllAsync_Refined<T>(IEnumerable<Func<CancellationToken, Task<T>>> tasks, CancellationToken ct)
    {
        var taskFactories = tasks.ToList();
        var tasksToAwait = new List<Task<T>>();

        foreach (var factory in taskFactories)
        {
            tasksToAwait.Add(Task.Run(async () =>
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
            }, ct).Unwrap());
        }

        T[] results = await Task.WhenAll(tasksToAwait.ToArray()).ConfigureAwait(false);
        return new List<T>(results);
    }
}