using System;
using System.Threading.Tasks;

public static class Loader
{
    public static async Task<string> LoadWithFallbackAsync(Task<string> primary, string fallback)
    {
        try
        {
            return await primary;
        }
        catch (Exception)
        {
            return fallback;
        }
    }
}