interface IDataLoader
{
    Task<string> LoadAsync(string source, CancellationToken ct);
}

class ParallelDataService
{
    private readonly IDataLoader loaderA;
    private readonly IDataLoader loaderB;
    private readonly IDataLoader loaderC;

    public ParallelDataService(IDataLoader loaderA, IDataLoader loaderB, IDataLoader loaderC)
    {
        this.loaderA = loaderA;
        this.loaderB = loaderB;
        this.loaderC = loaderC;
    }

    public Task<List<string>> LoadAllAsync(CancellationToken ct)
    {
        var taskA = loaderA.LoadAsync("sourceA", ct);
        var taskB = loaderB.LoadAsync("sourceB", ct);
        var taskC = loaderC.LoadAsync("sourceC", ct);

        return Task.WhenAll(taskA, taskB, taskC)
            .ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    throw t.Exception;
                }

                return new List<string> { taskA.Result, taskB.Result, taskC.Result };
            }, TaskScheduler.Default);
    }
}