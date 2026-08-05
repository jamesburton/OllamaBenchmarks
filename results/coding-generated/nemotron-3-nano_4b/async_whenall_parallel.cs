interface IDataLoader
{
    Task<string> LoadAsync(string source, CancellationToken ct);
}

public class ParallelDataService
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
        var tA = loaderA.LoadAsync("sourceA", ct);
        var tB = loaderB.LoadAsync("sourceB", ct);
        var tC = loaderC.LoadAsync("sourceC", ct);

        var result = await Task.WhenAll(tA, tB, tC);
        return new List<string> { result[0], result[1], result[2] };
    }
}