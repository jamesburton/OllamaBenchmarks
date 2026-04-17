using System.Threading.Tasks;

public static class ConfigureAwaitHelper
{
    public static async Task<string> FetchDataAsync()
    {
        return await Task.FromResult("data").ConfigureAwait(false);
    }
}