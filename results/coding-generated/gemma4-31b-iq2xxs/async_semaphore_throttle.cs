public async Task<List<T>> ProcessAllAsync<T>(IEnumerable<Func<CancellationHToken, Task<T>>> tasks, CancellationHToken ct) {
            var tasks = new List<Task<T>>();
            foreach (var factory in tasks) {
                // We need to wrap the factory to handle the semaphore
                // But we can't just call it; we need to create a task that’s’ a wrapper.
                // ...
            }
        }

public async Task<List<T>> ProcessAllAsync<T>(IEnumerable<Func<CancellationHToken, Task<T>>> tasks, CancellationHToken ct) {
            // ...
        }

public async Task<List<T>> ProcessAllAsync<T>(IEnumerable<Func<CancellationHToken, Task<T>>> tasks, CancellationHToken ct) {
            // ...
        }