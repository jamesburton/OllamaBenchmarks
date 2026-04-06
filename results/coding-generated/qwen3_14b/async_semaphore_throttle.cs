public class ThrottledProcessor
{
    private readonly SemaphoreSlim _semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        _semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public async Task<List<T>> ProcessAllAsync<T>(IEnumerable<Func<CancellationToken, Task<T>>> tasks, CancellationToken ct)
    {
        var tasksToRun = new List<Task<T>>();
        foreach (var taskFactory in tasks)
        {
            var task = Task.Factory.StartNew(async () =>
            {
                await _semaphore.WaitAsync(ct);
                try
                {
                    return await taskFactory(ct);
                }
                finally
                {
                    _semaphore.Release();
                }
            }, ct, TaskCreationOptions.None, TaskScheduler.Default).Unwrap();
            tasksToRun.Add(task);
        }

        await Task.WhenAll(tasksToRun);
        return tasksToRun.Select(t => t.Result).ToList();
    }
}

Wait, but the problem says that the method should return the results as a List<T> in the same order as the input task factories. The code above adds the tasks in the order of the input, and when we await them all, the results are in the same order. So that's correct.

But there's a problem with the code above. The Task.Factory.StartNew is creating a Task<Task<T>> which is then unwrapped. However, the Unwrap() method returns a Task<T>, which is added to the tasksToRun list. Then, when we await Task.WhenAll(tasksToRun), it will await all the Task<T> tasks, and collect their results.

But I need to make sure that the code is correct. Also, the use of Task.Factory.StartNew might be considered outdated, but given the constraints, perhaps it's