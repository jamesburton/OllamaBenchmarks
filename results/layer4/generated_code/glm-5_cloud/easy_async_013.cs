using System;
using System.Threading;
using System.Threading.Tasks;

public static class Timeout
{
    public static async Task<T?> WithTimeoutAsync<T>(Task<T> task, int timeoutMs) where T : class
    {
        if (task == null)
        {
            throw new ArgumentNullException(nameof(task));
        }

        // Use Task.WhenAny to wait for either the task or the timeout delay
        Task completedTask = await Task.WhenAny(task, Task.Delay(timeoutMs));

        if (completedTask == task)
        {
            // Task completed within the timeout
            return await task;
        }
        else
        {
            // Timeout occurred
            return null;
        }
    }
}