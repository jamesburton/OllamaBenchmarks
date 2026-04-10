public interface IExpensiveService
        {
            Task<string> ComputeAsync(string key);
        }