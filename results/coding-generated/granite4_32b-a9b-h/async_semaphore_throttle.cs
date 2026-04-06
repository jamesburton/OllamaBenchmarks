public class ThrottledProcessor
{
    private readonly SemaphoreSlim semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        this.semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public async Task<List<T>> ProcessAllAsync<T>(IEnumerable<Func<CancellationToken, Task<T>>> tasks, CancellationToken ct)
    {
        var results = await Task.WhenAll(tasks.Select(async taskFactory =>
        {
            try
            {
                await semaphore.WaitAsync(ct);
                return await taskFactory(ct);
            }
            finally
            {
                semaphore.Release();
            }
        }));

        return new List<T>(results);
    }
}