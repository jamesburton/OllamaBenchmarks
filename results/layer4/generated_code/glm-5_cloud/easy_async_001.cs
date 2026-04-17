using System;
using System.Threading.Tasks;

public static class Runner
{
    public static async Task<int> RunOnThreadPool(Func<int> func)
    {
        return await Task.Run(func);
    }
}