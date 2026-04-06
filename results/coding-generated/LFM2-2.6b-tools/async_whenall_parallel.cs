public interface IDataLoader
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

    public async Task<List<string>> LoadAllAsync(CancellationToken ct)
    {
        var results = await Task.WhenAll(new[]
        {
            loaderA.LoadAsync("sourceA", ct),
            loaderB.LoadAsync("sourceB", ct),
            loaderC.LoadAsync("sourceC", ct)
        });
        return results.Select(r => r).ToList();
    }
}