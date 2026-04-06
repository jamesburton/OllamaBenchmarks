public interface IDataLoader { Task<string> LoadAsync(string source, CancellationToken ct);}  
public class ParallelDataService {  
    private readonly IDataLoader _loaderA;  
    private readonly IDataLoader _loaderB;  
    private readonly IDataLoader _loaderC;  
    public ParallelDataService(IADataLoader a, IBDataLoader b, ICDataLoader c) {  
        _loaderA = a; _loaderB = b; _loaderC = c;  
    }  
    public async Task<List<string>> LoadAllAsync(CancellationToken ct) {  
        var results = await Task.WhenAll(  
            Task.Run(() => _loaderA.LoadAsync("sourceA", ct)),  
            Task.Run(() => _loaderB.LoadAsync("sourceB", ct)),  
            Task.Run(() => _loaderC.LoadAsync("sourceC", ct))  
        );  
        return results;  
    }  
}