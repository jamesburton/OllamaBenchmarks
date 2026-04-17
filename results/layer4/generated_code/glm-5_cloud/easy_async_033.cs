using System.Collections.Generic;
using System.Threading.Tasks;

public static class AsyncReader
{
    public static async IAsyncEnumerable<int> GenerateAsync(int start, int count)
    {
        for (int i = 0; i < count; i++)
        {
            await Task.Yield();
            yield return start + i;
        }
    }
}