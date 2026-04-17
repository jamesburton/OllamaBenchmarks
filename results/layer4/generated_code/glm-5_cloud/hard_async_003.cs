using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class AsyncSemaphorePool<T>
{
    private readonly SemaphoreSlim _semaphore;
    private readonly Queue<T> _resources;
    private readonly object _lock = new object();

    public AsyncSemaphorePool(IReadOnlyList<T> resources)
    {
        if (resources == null)
            throw new ArgumentNullException(nameof(resources));

        _resources = new Queue<T>(resources.Count);
        for (int i = 0; i < resources.Count; i++)
        {
            _resources.Enqueue(resources[i]);
        }

        _semaphore = new SemaphoreSlim(resources.Count, resources.Count);
    }

    public int AvailableCount
    {
        get
        {
            return _semaphore.CurrentCount;
        }
    }

    public async Task<T> AcquireAsync(CancellationToken ct)
    {
        await _semaphore.WaitAsync(ct).ConfigureAwait(false);

        lock (_lock)
        {
            return _resources.Dequeue();
        }
    }

    public void Release(T resource)
    {
        lock (_lock)
        {
            _resources.Enqueue(resource);
        }

        _semaphore.Release();
    }

    public async Task<TResult> UseAsync<TResult>(Func<T, Task<TResult>> action, CancellationToken ct)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        T resource = await AcquireAsync(ct).ConfigureAwait(false);
        try
        {
            return await action(resource).ConfigureAwait(false);
        }
        finally
        {
            Release(resource);
        }
    }
}