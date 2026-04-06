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
        Task<string> taskA = loaderA.LoadAsync("sourceA", ct);
        Task<string> taskB = loaderB.LoadAsync("sourceB", ct);
        Task<string> taskC = loaderC.LoadAsync("sourceC", ct);

        return Task.WhenAll(taskA, taskB, taskC)
            .ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    throw t.Exception;
                }

                return new List<string> { t.Result[0], t.Result[1], t.Result[2] };
            }, TaskContinuationOptions.OnlyOnSuccessful);
    }
}