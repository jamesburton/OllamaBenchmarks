class ThrottledProcessor 
{
    private readonly System.Threading.SemaphoreSlim _semaphore;

    public ThrottledProcessor(int maxConcurrent) 
    {
        _semaphore = new System.Threading.SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public async System.Threading.Tasks.Task<List<T>>> ProcessAllAsync<T>(System.Collections.Generic.IEnumerable<System.Func<System.Threading.CancellationToken, System.Threading.Tasks.Task<T>>> tasks, System.Threading.CancellationToken ct) 
    {
        var taskList = tasks.Select(factory => 
            System.Threading.Tasks.Task.Run(async () => 
            {
                await _semaphore.WaitAsync(ct);
                try 
                {
                    return await factory(ct);
                }
                finally 
                {
                    _semaphore.Release();
                }
            })
        ).ToList();

        var results = await System.Threading.Tasks.Task.WhenAll(taskList);
        return results.ToList();
    }
}