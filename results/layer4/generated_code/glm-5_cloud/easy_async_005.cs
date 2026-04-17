using System;
using System.Threading;
using System.Threading.Tasks;

public static class Cancellable
{
    public static async Task<string> GetValueAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(100, cancellationToken);
        return "done";
    }
}