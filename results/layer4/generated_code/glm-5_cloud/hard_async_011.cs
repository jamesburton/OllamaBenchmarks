using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

public class AsyncPriorityQueue
{
    private readonly object _lock = new object();
    private readonly List<(T Item, int Priority)> _items = new List<(T, int)>();
    private readonly Queue<TaskCompletionSource<T>> _waitingReaders = new Queue<TaskCompletionSource<T>>();
    private bool _completed;

    public AsyncPriorityQueue() { }

    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _items.Count;
            }
        }
    }

    public void Complete()
    {
        lock (_lock)
        {
            if (_completed) return;
            _completed = true;

            // If queue is empty, fail all waiting readers.
            // If queue is not empty, waiting readers should ideally be cancelled or failed,
            // but strict priority queue semantics usually imply that if items exist, they are consumed.
            // However, Complete() means "no more items will be added".
            // If items exist, DequeueAsync should still return them until empty.
            // If items exist but there are waiting readers (which implies items < readers),
            // the excess readers need to be failed.

            // Note: _items.Count should be less than _waitingReaders.Count if readers are waiting.
            // We fail the readers that won't get an item.
            while (_waitingReaders.Count > _items.Count)
            {
                var tcs = _waitingReaders.Dequeue();
                tcs.TrySetException(new ChannelClosedException());
            }
        }
    }

    public Task EnqueueAsync(T item, int priority, CancellationToken ct)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));

        lock (_lock)
        {
            if (_completed)
            {
                throw new ChannelClosedException("Queue has been completed.");
            }

            ct.ThrowIfCancellationRequested();

            if (_waitingReaders.Count > 0)
            {
                // An item is needed immediately.
                // We must maintain priority order even when satisfying waiting readers.
                // We add the item to the buffer, sort, and then satisfy the first waiter with the best item.

                _items.Add((item, priority));
                _items.Sort((x, y) => y.Priority.CompareTo(x.Priority));

                var tcs = _waitingReaders.Dequeue();
                var bestItem = _items[0];
                _items.RemoveAt(0);

                // We cannot guarantee the order of completion vs cancellation registration below,
                // but TrySetResult is safe.
                tcs.TrySetResult(bestItem);
            }
            else
            {
                // No waiters, just add to buffer.
                _items.Add((item, priority));
                _items.Sort((x, y) => y.Priority.CompareTo(x.Priority));
            }

            return Task.CompletedTask;
        }
    }

    public Task DequeueAsync(CancellationToken ct)
    {
        lock (_lock)
        {
            if (_items.Count > 0)
            {
                var item = _items[0];
                _items.RemoveAt(0);
                return Task.FromResult(item);
            }

            if (_completed)
            {
                throw new ChannelClosedException();
            }

            var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
            _waitingReaders.Enqueue(tcs);

            // Registration for cancellation.
            // This creates a closure capturing 'tcs'.
            ct.Register(() =>
            {
                // We try to remove it. If it's gone, it was already processed.
                // Since we can't efficiently remove from a Queue without locking and iterating,
                // we just TrySetCanceled. If it was already set, this does nothing.
                tcs.TrySetCanceled(ct);
            });

            return tcs.Task;
        }
    }

    public Task<T[]> DrainAsync(CancellationToken ct)
    {
        lock (_lock)
        {
            if (_completed && _items.Count == 0)
            {
                throw new ChannelClosedException();
            }

            ct.ThrowIfCancellationRequested();

            if (_items.Count == 0)
            {
                return Array.Empty<T>();
            }

            var result = new T[_items.Count];
            for (int i = 0; i < _items.Count; i++)
            {
                result[i] = _items[i].Item;
            }

            _items.Clear();
            return Task.FromResult(result);
        }
    }
}