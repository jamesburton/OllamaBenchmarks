using System;
using System.Threading.Tasks;

public static class Completer
{
    public static async Task WithCompletionSource(Action<TaskCompletionSource> configure)
    {
        var tcs = new TaskCompletionSource();
        configure(tcs);
        return await tcs.Task;
    }
}