public class ThrottledProcessor
{
    private readonly SemaphoreSlim _semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        _semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public async Task<List<T>> ProcessAllAsync<T>(IEnumerable<Func<CancellationToken, Task<T>>> tasks, CancellationToken ct)
    {
        var results = new List<T>(tasks.Count());

        var tasksToRun = new List<Task>();
        var enumerator = tasks.GetEnumerator();
        foreach (var factory in enumerator)
        {
            tasksToRun.Add(Task.Run(() => {
                try
                {
                    _ = _semaphore.WaitAsync(ct);
                    var result = factory(ct);
                    _semaphore.Release();
                }
                catch (OperationCanceledException)
                {
                }
                finally
                {
                    _semaphore.Release();
                }
            }));
        }

        var resultsArray = await Task.WhenAll(tasksToRun);
        foreach (var result in resultsArray)
            results.Add(result);

        return results;
    }
}