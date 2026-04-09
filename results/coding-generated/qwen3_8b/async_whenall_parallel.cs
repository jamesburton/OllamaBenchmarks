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

    public async Task<List<string>> LoadAllAsync(CancellationToken ct)
    {
        var tasks = new[]
        {
            loaderA.LoadAsync("sourceA", ct),
            loaderB.LoadAsync("sourceB", ct),
            loaderC.LoadAsync("sourceC", ct)
        };

        var results = await Task.WhenAll(tasks);
        return results.ToList();
    }
}