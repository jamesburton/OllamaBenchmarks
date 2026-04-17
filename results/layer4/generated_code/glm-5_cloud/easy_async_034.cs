using System;
using System.Threading.Tasks;

public static class TaskConverter
{
    public static async Task<string> NumberToStringAsync(int number)
    {
        await Task.Delay(100); // Simulating asynchronous work
        return $"Number: {number}";
    }
}