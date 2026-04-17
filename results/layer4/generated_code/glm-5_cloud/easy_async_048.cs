using System.Collections.Generic;
using System.Threading.Tasks;

public static class AsyncLookup
{
    public static async Task<bool> ExistsAsync<T>(IEnumerable<T> items, Func<T, Task<bool>> predicate)
    {
        foreach (var item in items)
        {
            if (await predicate(item))
            {
                return true;
            }
        }
        return false;
    }
}