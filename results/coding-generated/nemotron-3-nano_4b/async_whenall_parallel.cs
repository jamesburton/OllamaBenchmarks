interface IDataLoader
{
    Task<string> LoadAsync(string source, CancellationToken ct);
}

class ParallelDataService
{
    IDataLoader loaderA;
    IDataLoader loaderB;
    IDataLoader loaderC;

    public ParallelDataService(IDataLoader loaderA, IDataLoader loaderB, IDataLoader loaderC)
    {
        this.loaderA = loaderA;
        this.loaderB = loaderB;
        this.loaderC = loaderC;
    }

    public async Task<List<string>> LoadAllAsync(CancellationToken ct)
    {
        var taskA = loaderA.LoadAsync("sourceA", ct);
        var taskB = loaderB.LoadAsync("sourceB", ct);
        var taskC = loaderC.LoadAsync("sourceC", ct);
        return await Task.WhenAll(taskA, taskB, taskC).ToList();
    }
}