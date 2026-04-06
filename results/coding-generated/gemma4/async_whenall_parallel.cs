interface IDataLoader
{
    Task<string> LoadAsync(string source, CancellationToken ct);
}

class ParallelDataService
{
    private readonly IDataLoader _loaderA;
    private readonly IDataLoader _loaderB;
    private readonly IDataLoader _loaderC;

    public ParallelDataService(IDataLoader loaderA, IDataLoader loaderB, IDataLoader loaderC)
    {
        _loaderA = loaderA;
        _loaderB = loaderB;
        _loaderC = loaderC;
    }

    public async Task<List<string>> LoadAllAsync(CancellationToken ct)
    {
        Task<string> taskA = _loaderA.LoadAsync("sourceA", ct);
        Task<string> taskB = _loaderB.LoadAsync("sourceB", ct);
        Task<string> taskC = _loaderC.LoadAsync("sourceC", ct);

        await Task.WhenAll(taskA, taskB, taskC);

        return new List<string> { await taskA, await taskB, await taskC };
    }
}