using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace VerticalSlice
{
    /// <summary>
    /// An in-process publish-subscribe event bus that supports async handlers.
    /// </summary>
    public class AsyncEventBus
    {
        // Thread-safe dictionary to hold subscriptions.
        // Key: Message Type (typeof(T)), Value: List of Delegate wrappers.
        private readonly ConcurrentDictionary<Type, List<DelegateWrapper>> _subscriptions = new();

        /// <summary>
        /// Registers an async handler for a specific message type.
        /// </summary>
        public void Subscribe<T>(Func<T, CancellationToken, Task> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            var wrapper = new DelegateWrapper<T>(handler);
            var type = typeof(T);

            // Get or create the list of handlers for this type in a thread-safe manner.
            var list = _subscriptions.GetOrAdd(type, _ => new List<DelegateWrapper>());

            // Lock on the specific list to ensure thread-safe addition.
            lock (list)
            {
                list.Add(wrapper);
            }
        }

        /// <summary>
        /// Removes a handler for a specific message type.
        /// </summary>
        public void Unsubscribe<T>(Func<T, CancellationToken, Task> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            var type = typeof(T);

            if (_subscriptions.TryGetValue(type, out var list))
            {
                // Lock on the specific list to ensure thread-safe removal.
                lock (list)
                {
                    // Find the wrapper that matches the delegate.
                    // We must match the target and method, standard delegate equality logic.
                    var toRemove = list.FirstOrDefault(w => w.Matches(handler));
                    if (toRemove != null)
                    {
                        list.Remove(toRemove);
                    }
                }
            }
        }

        /// <summary>
        /// Invokes all handlers for type T concurrently.
        /// Collects all exceptions into AggregateException if any fail.
        /// </summary>
        public async Task PublishAsync<T>(T message, CancellationToken ct)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            var handlers = GetHandlers<T>();
            if (handlers.Count == 0) return;

            // Start all tasks concurrently.
            var tasks = handlers.Select(h => h.Invoke(message, ct)).ToList();

            // Wait for all to complete. Task.WhenAll wraps exceptions in AggregateException automatically.
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        /// <summary>
        /// Invokes handlers one at a time in registration order.
        /// Stops and throws on first failure.
        /// </summary>
        public async Task PublishSequentialAsync<T>(T message, CancellationToken ct)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            var handlers = GetHandlers<T>();

            foreach (var handler in handlers)
            {
                ct.ThrowIfCancellationRequested();
                await handler.Invoke(message, ct).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Returns the number of registered handlers for type T.
        /// </summary>
        public int HandlerCount<T>()
        {
            var type = typeof(T);
            if (_subscriptions.TryGetValue(type, out var list))
            {
                lock (list)
                {
                    return list.Count;
                }
            }
            return 0;
        }

        // Helper to get a snapshot of handlers for type T.
        private List<Func<T, CancellationToken, Task>> GetHandlers<T>()
        {
            var type = typeof(T);
            if (_subscriptions.TryGetValue(type, out var list))
            {
                lock (list)
                {
                    // Create a copy of the handlers list to avoid holding the lock during execution.
                    return list
                        .Cast<DelegateWrapper<T>>()
                        .Select(w => w.Handler)
                        .ToList();
                }
            }
            return new List<Func<T, CancellationToken, Task>>();
        }

        // Abstract base class to allow storing different generic types in the same list.
        private abstract class DelegateWrapper
        {
            public abstract bool Matches(Delegate d);
        }

        // Strongly typed wrapper to store the handler.
        private class DelegateWrapper<T> : DelegateWrapper
        {
            public Func<T, CancellationToken, Task> Handler { get; }

            public DelegateWrapper(Func<T, CancellationToken, Task> handler)
            {
                Handler = handler;
            }

            public Task Invoke(T message, CancellationToken ct) => Handler(message, ct);

            public override bool Matches(Delegate d)
            {
                return Handler.Equals(d);
            }
        }
    }
}