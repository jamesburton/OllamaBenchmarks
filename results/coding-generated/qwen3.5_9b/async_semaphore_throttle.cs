public class ThrottledProcessor
{
    private readonly SemaphoreSlim _semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        if (maxConcurrent <= 0)
            throw new ArgumentException("maxConcurrent must be greater than zero", nameof(maxConcurrent));

        _semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public async Task<List<T>> ProcessAllAsync<T>(IEnumerable<Func<CancellationToken, Task<T>>> tasks, CancellationToken ct)
    {
        var taskList = new List<Task<T>>(tasks.Count());

        foreach (var factory in tasks)
        {
            await _semaphore.WaitAsync(ct);

            try
            {
                // Launch the work immediately without awaiting to allow concurrency up to maxConcurrent
                Task<T> currentTask;

                if (factory != null)
                    currentTask = factory(ct);
                else
                    throw new InvalidOperationException("Task factory cannot be null");

                taskList.Add(currentTask);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        // Await all tasks concurrently and collect results in order (they are added to list in input order)
        var completedTasks = await Task.WhenAll(taskList.ToArray());

        return new List<T>(completedTasks);
    }
}