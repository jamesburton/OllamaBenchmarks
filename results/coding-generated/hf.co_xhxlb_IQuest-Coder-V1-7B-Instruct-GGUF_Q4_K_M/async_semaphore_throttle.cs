using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class ThrottledProcessor
{
    private readonly SemaphoreSlim _semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        _semaphore = new SemaphoreSlim(maxConcurrent);
    }

    public async Task<List<T>> ProcessAllAsync<T>(IEnumerable<Func<CancellationToken, Task<T>>> tasks, CancellationToken ct)
    {
        var results = new List<T>();
        var list = tasks.ToList(); // Get the order of tasks

        // We need to process tasks in order and return results in order
        // To do this, we can use a concurrent dictionary or just a list of tasks
        // Since we need to return in the same order as input, we can use a lock or a concurrent collection

        // However, the simplest way to ensure order is to process them sequentially
        // but using the semaphore to throttle the concurrency.

        // Wait for the semaphore to be available
        await _semaphore.WaitAsync(ct);
        try
        {
            var tasksList = new List<Task<T>>();

            foreach (var taskFactory in list)
            {
                var task = taskFactory(ct);
                tasksList.Add(task);
            }

            // Wait for all tasks to complete
            var completedTasks = await Task.WhenAll(tasksList);

            // Return results in the same order as input
            for (int i = 0; i < list.Count; i++)
            {
                results.Add(completedTasks[i]);
            }
        }
        finally
        {
            _semaphore.Release();
        }

        return results;
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class ThrottledProcessor
{
    private readonly SemaphoreSlim _semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        _semaphore = new SemaphoreSlim(maxConcurrent);
    }

    public async Task<List<T>> ProcessAllAsync<T>(IEnumerable<Func<CancellationToken, Task<T>>> tasks, CancellationToken ct)
    {
        var results = new List<T>();
        var list = tasks.ToList();

        // Process tasks sequentially to maintain order, but throttle concurrency
        foreach (var taskFactory in list)
        {
            // Acquire semaphore
            await _semaphore.WaitAsync(ct);
            try
            {
                // Execute the task
                var result = await taskFactory(ct);
                results.Add(result);
            }
            finally
            {
                // Release semaphore
                _semaphore.Release();
            }
        }

        return results;
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class ThrottledProcessor
{
    private readonly SemaphoreSlim _semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        _semaphore = new SemaphoreSlim(maxConcurrent);
    }

    public async Task<List<T>> ProcessAllAsync<T>(IEnumerable<Func<CancellationToken, Task<T>>> tasks, CancellationToken ct)
    {
        var results = new List<T>();
        var list = tasks.ToList();

        // Process sequentially to maintain order, but throttle concurrency
        foreach (var taskFactory in list)
        {
            // Acquire semaphore
            await _semaphore.WaitAsync(ct);
            try
            {
                // Execute the task
                var result = await taskFactory(ct);
                results.Add(result);
            }
            finally
            {
                // Release semaphore
                _semaphore.Release();
            }
        }

        return results;
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class ThrottledProcessor
{
    private readonly SemaphoreSlim _semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        _semaphore = new SemaphoreSlim(maxConcurrent);
    }

    public async Task<List<T>> ProcessAllAsync<T>(IEnumerable<Func<CancellationToken, Task<T>>> tasks, CancellationToken ct)
    {
        var results = new List<T>();
        var list = tasks.ToList();

        // Process sequentially to maintain order, but throttle concurrency
        foreach (var taskFactory in list)
        {
            // Acquire semaphore
            await _semaphore.WaitAsync(ct);
            try
            {
                // Execute the task
                var result = await taskFactory(ct);
                results.Add(result);
            }
            finally
            {
                // Release semaphore
                _semaphore.Release();
            }
        }

        return results;
    }
}