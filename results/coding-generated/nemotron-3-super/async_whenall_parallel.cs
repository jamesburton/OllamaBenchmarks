interface IDataLoader
{
    System.Threading.Tasks.Task<string> LoadAsync(string source, System.Threading.CancellationToken ct);
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

    public System.Threading.Tasks.Task<System.Collections.Generic.List<string>> LoadAllAsync(System.Threading.CancellationToken ct)
    {
        var taskA = _loaderA.LoadAsync("sourceA", ct);
        var taskB = _loaderB.LoadAsync("sourceB", ct);
        var taskC = _loaderC.LoadAsync("sourceC", ct);

        return System.Threading.Tasks.Task.WhenAll(taskA, taskB, taskC)
            .ContinueWith(t => 
                new System.Collections.Generic.List<string> { t.Result[0], t.Result[1], t.Result[2] },
                System.Threading.Tasks.TaskContinuationOptions.ExecuteSynchronously);
    }
}