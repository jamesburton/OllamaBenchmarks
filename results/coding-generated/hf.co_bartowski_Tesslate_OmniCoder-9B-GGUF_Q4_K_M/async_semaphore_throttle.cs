public class ThrottledProcessor
{
    private readonly SemaphoreSlim _semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        _semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public async Task<List<T>> ProcessAllAsync<T>(IEnumerable<Func<CancellationToken, Task<T>>> tasks, CancellationToken ct)
    {
        var result = new List<T>();
        var taskList = new List<Task>();

        foreach (var taskFactory in tasks)
        {
            await _semaphore.WaitAsync(ct);
            try
            {
                taskList.Add(Task.Run(async () =>
                {
                    var token = ct;
                    var task = taskFactory(token);
                    var item = await task;
                    result.Add(item);
                }));
            }
            finally
            {
                _semaphore.Release();
            }
        }

        await Task.WhenAll(taskList);
        return result;
    }
}

public class ThrottledProcessor
{
    private readonly SemaphoreSlim _semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        _semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public async Task<List<T>> ProcessAllAsync<T>(IEnumerable<Func<CancellationToken, Task<T>>> tasks, CancellationToken ct)
    {
        var taskList = new List<Task>();
        var result = new List<T>();
        var index = 0;

        foreach (var taskFactory in tasks)
        {
            await _semaphore.WaitAsync(ct);
            try
            {
                taskList.Add(Task.Run(async () =>
                {
                    var token = ct;
                    var task = taskFactory(token);
                    var item = await task;
                    result.Add(item);
                }));
            }
            finally
            {
                _semaphore.Release();
            }
        }

        await Task.WhenAll(taskList);
        return result;
    }
}

public class ThrottledProcessor
{
    private readonly SemaphoreSlim _semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        _semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public async Task<List<T>> ProcessAllAsync<T>(IEnumerable<Func<CancellationToken, Task<T>>> tasks, CancellationToken ct)
    {
        var taskList = new List<Task>();
        var result = new T[tasks.Count()];
        var index = 0;

        foreach (var taskFactory in tasks)
        {
            await _semaphore.WaitAsync(ct);
            try
            {
                taskList.Add(Task.Run(async () =>
                {
                    var token = ct;
                    var task = taskFactory(token);
                    var item = await task;
                    result[index++] = item;
                }));
            }
            finally
            {
                _semaphore.Release();
            }
        }

        await Task.WhenAll(taskList);
        return result.ToList();
    }
}

public class ThrottledProcessor
{
    private readonly SemaphoreSlim _semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        _semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public async Task<List<T>> ProcessAllAsync<T>(IEnumerable<Func<CancellationToken, Task<T>>> tasks, CancellationToken ct)
    {
        var taskFactories = tasks.ToList();
        var taskList = new List<Task>();
        var result = new List<T>();
        var index = 0;

        foreach (var taskFactory in taskFactories)
        {
            await _semaphore.WaitAsync(ct);
            try
            {
                taskList.Add(Task.Run(async () =>
                {
                    var token = ct;
                    var task = taskFactory(token);
                    var item = await task;
                    result.Add(item);
                }));
            }
            finally


public class ThrottledProcessor
{
    private readonly SemaphoreSlim _semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        _semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public async Task<List<T>> ProcessAllAsync<T>(IEnumerable<Func<CancellationToken, Task<T>>> tasks, CancellationToken ct)
    {
        var taskFactories = tasks.ToList();
        var taskList = new List<Task>();
        var result = new T[taskFactories.Count];
        var index = 0;

        foreach (var taskFactory in taskFactories)
        {
            await _semaphore.WaitAsync(ct);
            try
            {
                taskList.Add(Task.Run(async () =>
                {
                    var token = ct;
                    var task = taskFactory(token);
                    var item = await task;
                    result[index++] = item;
                }));
            }
            finally
            {
                _semaphore.Release();
            }
        }

        await Task.WhenAll(taskList);
        return result.ToList();
    }
}

public class ThrottledProcessor
{
    private readonly SemaphoreSlim _semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        _semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public async Task<List<T>> ProcessAllAsync<T>(IEnumerable<Func<CancellationToken, Task<T>>> tasks, CancellationToken ct)
    {
        var taskFactories = tasks.ToList();
        var taskList = new List<Task>();
        var result = new T[taskFactories.Count];
        var index = 0;

        foreach (var taskFactory in taskFactories)
        {
            await _semaphore.WaitAsync(ct);
            try
            {
                taskList.Add(Task.Run(async () =>
                {
                    var token = ct;
                    var task = taskFactory(token);
                    var item = await task;
                    result[index++] = item;
                }));
            }
            finally
            {
                _semaphore.Release();
            }
        }

        await Task.WhenAll(taskList);
        return result.ToList();
    }
}

public class ThrottledProcessor
{
    private readonly SemaphoreSlim _semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        _semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public async Task<List<T>> ProcessAllAsync<T>(IEnumerable<Func<CancellationToken, Task<T>>> tasks, CancellationToken ct)
    {
        var taskFactories = tasks.ToList();
        var taskList = new List<Task>();
        var result = new T[taskFactories.Count];
        var index = 0;

        foreach (var taskFactory in taskFactories)
        {
            await _semaphore.WaitAsync(ct);
            try
            {
                var token = ct;
                var task = taskFactory(token);
                var item = await task;
                result[index++] = item;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        return result.ToList();
    }
}

public class ThrottledProcessor
{
    private readonly SemaphoreSlim _semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        _semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public async Task<List<T>> ProcessAllAsync<T>(IEnumerable<Func<CancellationToken, Task<T>>> tasks, CancellationToken ct)
    {
        var taskFactories = tasks.ToList();
        var taskList = new List<Task>();
        var result = new T[taskFactories.Count];
        var index = 0;

        foreach (var taskFactory in taskFactories)
        {
            await _semaphore.WaitAsync(ct);
            try
            {
                taskList.Add(Task.Run(async () =>
                {
                    var token = ct;
                    var task = taskFactory(token);
                    var item = await task;
                    result[index++] = item;
                }));
            }
            finally
            {
                _semaphore.Release();
            }
        }

        await Task.WhenAll(taskList);
        return result.ToList();
    }
}

public class ThrottledProcessor
{
    private readonly SemaphoreSlim _semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        _semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public async Task<List<T>> ProcessAllAsync<T>(IEnumerable<Func<CancellationToken, Task<T>>> tasks, CancellationToken ct)
    {
        var taskFactories = tasks.ToList();
        var taskList = new List<Task>();
        var result = new T[taskFactories.Count];
        var index = 0;

        foreach (var taskFactory in taskFactories)
        {
            await _semaphore.WaitAsync(ct);
            try
            {
                taskList.Add(Task.Run(async () =>
                {
                    var token = ct;
                    var task = taskFactory(token);
                    var item = await task;
                    result[index++] = item;
                }));
            }
            finally
            {
                _semaphore.Release();
            }
        }

        await Task.WhenAll(taskList);
        return result.ToList();
    }
}