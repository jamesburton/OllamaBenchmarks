using System;
using System.Threading;
using System.Threading.Tasks;

public static class TaskGuard
{
    public static async Task RunWithCancellationAsync(Func<Task> action, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        await action();
    }
}