using System.Threading.Tasks;

public static class AsyncFibonacci
{
    public static async Task<int> FibAsync(int n)
    {
        if (n <= 1)
        {
            return await Task.FromResult(n);
        }

        var task1 = FibAsync(n - 1);
        var task2 = FibAsync(n - 2);

        return await Task.WhenAll(task1, task2).ContinueWith(t => task1.Result + task2.Result);
    }
}