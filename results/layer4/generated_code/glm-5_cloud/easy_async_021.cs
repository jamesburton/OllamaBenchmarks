using System.Threading;
using System.Threading.Tasks;

public static class CancellationHelper
{
    public static async Task<bool> IsCancelledAsync(CancellationToken token)
    {
        // Check if cancellation has been requested.
        // We use await Task.CompletedTask to satisfy the async method signature
        // without introducing any actual delay or blocking behavior.
        await Task.CompletedTask;

        return token.IsCancellationRequested;
    }
}