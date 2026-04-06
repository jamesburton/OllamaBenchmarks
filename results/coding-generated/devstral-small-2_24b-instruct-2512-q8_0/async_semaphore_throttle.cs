using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class ThrottledProcessor
{
    private readonly SemaphoreSlim _semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        _semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public async Task<List<T>> ProcessAllAsync<T>(IEnumerable<Func<CancellationToken, Task<T>>> tasks, CancellationToken ct)
    {
        var taskFactories = new List<Func<CancellationToken, Task<T>>>(tasks);
        var results = new T[taskFactories.Count];
        var exceptions = new Exception[taskFactories.Count];
        var tasksArray = new Task[taskFactories.Count];

        for (int i = 0; i < taskFactories.Count; i++)
        {
            int index = i;
            tasksArray[i] = Task.Run(async () =>
            {
                await _semaphore.WaitAsync(ct);
                try
                {
                    results[index] = await taskFactories[index](ct);
                }
                catch (Exception ex)
                {
                    exceptions[index] = ex;
                }
                finally
                {
                    _semaphore.Release();
                }
            }, ct);
        }

        await Task.WhenAll(tasksArray);

        var resultList = new List<T>(taskFactories.Count);
        for (int i = 0; i < tasksArray.Length; i++)
        {
            if (exceptions[i] != null)
            {
                throw new AggregateException(exceptions);
            }
            resultList.Add(results[i]);
        }

        return resultList;
    }
}