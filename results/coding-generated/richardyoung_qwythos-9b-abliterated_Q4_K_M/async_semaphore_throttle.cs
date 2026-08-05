public class ThrottledProcessor {
    private readonly SemaphoreSlim _semaphore;

    public ThrottledProcessor(int maxConcurrent) {
        _semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public async Task<List<T>> ProcessAllAsync<T>(IEnumerable<Func<CancellationToken, Task<T>>> tasks, CancellationToken ct) {
        var indexedTasks = tasks.Select((factory, index) => (index, factory)).ToArray();

        var results = await Task.WhenAll(
            indexedTasks.Select(async item => {
                var semaphore = await _semaphore.WaitAsync(ct);
                try {
                    var result = await item.factory(ct);
                    return (Index: item.index, Result: result);
                } finally {
                    semaphore.Release();
                }
            })
        );

        return results.OrderBy(r => r.Index).Select(r => r.Result).ToList();
    }
}