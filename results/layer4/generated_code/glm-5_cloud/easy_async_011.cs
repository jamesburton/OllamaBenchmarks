using System;
using System.Threading.Tasks;

public static class SafeRunner
{
    public static async Task<bool> TryRunAsync(Func<Task> action)
    {
        try
        {
            await action().ConfigureAwait(false);
            return true;
        }
        catch
        {
            return false;
        }
    }
}