using System.Collections.Generic;
using System.Threading.Tasks;

public static class AsyncCollector
{
    public static async Task<List<T>> ToListAsync<T>(IAsyncEnumerable<T> source)
    {
        var list = new List<T>();

        await foreach (var item in source)
        {
            list.Add(item);
        }

        return list;
    }
}