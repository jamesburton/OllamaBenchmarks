public class ThrottledProcessor
{
    private readonly System.Threading.SemaphoreSlim _semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        _semaphore = new System.Threading.SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public async System.Threading.Tasks.Task<System.Collections.Generic.List<T>> ProcessAllAsync<T>(
        System.Collections.Generic.IEnumerable<System.Func<System.Threading.CancellationToken, System.Threading.Tasks.Task<T>>> tasks,
        System.Threading.CancellationToken ct)
    {
        var taskList = new System.Collections.Generic.List<System.Threading.Tasks.Task<(int index, T result)>>();

        int i = 0;
        foreach (var factory in tasks)
        {
            int currentIndex = i;
            var wrapperTask = System.Threading.Tasks.Task.Run(async () =>
            {
                await _semaphore.WaitAsync(ct).ConfigureAwait(false);
                try
                {
                    var res = await factory(ct).ConfigureAwait(false);
                    return (currentIndex, res);
                }
                finally
                {
                    _semaphore.Release();
                }
            }, ct);

            taskList.Add(wrapperTask);
            i++;
        }

        var results = await System.Threading.Tasks.Task.WhenAll(taskList).ConfigureAwait(false);

        var ordered = new T[results.Length];
        foreach (var tuple in results)
        {
            ordered[tuple.index] = tuple.result;
        }

        return new System.Collections.Generic.List<T>(ordered);
    }
}