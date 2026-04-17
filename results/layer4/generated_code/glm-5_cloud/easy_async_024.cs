using System.Collections.Generic;
using System.Threading.Tasks;

public static class AsyncCollector
{
    public static async Task<int> CountAsync<T>(IAsyncEnumerable<T> source)
    {
        int count = 0;

        await foreach (var item in source)
        {
            count++;
        }

        return count;
    }
}