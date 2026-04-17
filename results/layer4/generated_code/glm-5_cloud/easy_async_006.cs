using System.Collections.Generic;
using System.Threading.Tasks;

public static class Fetcher
{
    public static async Task<List<int>> FetchAllAsync(IEnumerable<Task<int>> tasks)
    {
        int[] results = await Task.WhenAll(tasks);
        return new List<int>(results);
    }
}