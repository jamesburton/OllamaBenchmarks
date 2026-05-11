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
        Task<string> taskA = loaderA.LoadAsync("sourceA", ct);
        Task<string> taskB = loaderB.LoadAsync("sourceB", ct);
        Task<string> taskC = loaderC.LoadAsync("sourceC", ct);

        Task<string>[] tasks = new Task<string>[] { taskA, taskB, taskC };
        string[] results = await Task.WhenAll(tasks);

        return new List<string> { results[0], results[1], results[2] };
    }
}