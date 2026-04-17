using System;
using System.Threading.Tasks;

public static class AsyncConditional
{
    public static async Task<TResult> IfElseAsync<TResult>(bool condition, Func<Task<TResult>> ifTrue, Func<Task<TResult>> ifFalse)
    {
        if (condition)
        {
            return await ifTrue().ConfigureAwait(false);
        }
        else
        {
            return await ifFalse().ConfigureAwait(false);
        }
    }
}