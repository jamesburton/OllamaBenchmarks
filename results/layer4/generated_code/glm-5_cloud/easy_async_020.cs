using System;
using System.Threading.Tasks;

public static class AsyncPipeline
{
    public static async Task<TResult> PipeAsync<T, TResult>(Task<T> source, Func<T, Task<TResult>> transform)
    {
        T result = await source.ConfigureAwait(false);
        return await transform(result).ConfigureAwait(false);
    }
}