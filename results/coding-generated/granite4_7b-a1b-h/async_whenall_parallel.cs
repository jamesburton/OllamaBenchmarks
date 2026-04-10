interface IDataLoader
{
    Task<string> LoadAsync(string source, CancellationToken ct);
}

class ParallelDataService
{
    private IDataLoader loaderA;
    private IDataLoader loaderB;
    private IDataLoader loaderC;

    public ParallelDataService(IDataLoader loaderA, IDataLoader loaderB, IDataLoader loaderC)
    {
        this.loaderA = loaderA;
        this.loaderB = loaderB;
        this.loaderC = loaderC;
    }

    public Task<List<string>> LoadAllAsync(CancellationToken ct)
    {
        var tasks = new[]
        {
            loaderA.LoadAsync("sourceA", ct),
            loaderB.LoadAsync("sourceB", ct),
            loaderC.LoadAsync("sourceC", ct)
        };

        return Task.WhenAll(tasks).ContinueWith(t =>
        {
            return new List<string> { t.Result[0], t.Result[1], t.Result[2] };
        });
    }
}