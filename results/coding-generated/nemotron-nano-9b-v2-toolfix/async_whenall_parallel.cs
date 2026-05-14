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
        var resultA = await loaderA.LoadAsync("sourceA", ct);
        var resultB = await loaderB.LoadAsync("sourceB", ct);
        var resultC = await loaderC.LoadAsync("sourceC", ct);
        return new List<string> { resultA, resultB, resultC };
    }
}