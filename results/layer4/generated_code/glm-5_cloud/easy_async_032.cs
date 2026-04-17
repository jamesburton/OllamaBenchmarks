using System;
using System.Threading.Tasks;

public static class AsyncValidator
{
    public static async Task<bool> ValidateAsync(string input, Func<string, Task<bool>> validator)
    {
        return await validator(input);
    }
}