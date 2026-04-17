using System;
using System.Threading.Tasks;

public static class DelayedGreeter
{
    public static async Task<string> GreetAfterDelayAsync(string name, int delayMs)
    {
        await Task.Delay(delayMs);
        return $"Hello, {name}!";
    }
}