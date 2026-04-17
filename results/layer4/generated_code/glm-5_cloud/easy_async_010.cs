using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public static class Mapper
{
    public static async Task<List<TResult>> SelectAsync<T, TResult>(IEnumerable<T> items, Func<T, Task<TResult>> selector)
    {
        if (items == null)
        {
            throw new ArgumentNullException(nameof(items));
        }

        if (selector == null)
        {
            throw new ArgumentNullException(nameof(selector));
        }

        var tasks = new List<Task<TResult>>();

        foreach (var item in items)
        {
            tasks.Add(selector(item));
        }

        TResult[] results = await Task.WhenAll(tasks).ConfigureAwait(false);

        return new List<TResult>(results);
    }
}