using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public static class AsyncFilter
{
    public static async Task<List<T>> WhereAsync<T>(IEnumerable<T> items, Func<T, Task<bool>> predicate)
    {
        var result = new List<T>();

        foreach (var item in items)
        {
            if (await predicate(item).ConfigureAwait(false))
            {
                result.Add(item);
            }
        }

        return result;
    }
}