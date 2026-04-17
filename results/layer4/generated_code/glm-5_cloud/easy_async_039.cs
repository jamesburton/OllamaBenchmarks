using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public static class Accumulator
{
    public static async Task<T> ReduceAsync<T>(IEnumerable<Task<T>> tasks, T seed, Func<T, T, T> combine)
    {
        if (tasks == null)
        {
            throw new ArgumentNullException(nameof(tasks));
        }
        if (combine == null)
        {
            throw new ArgumentNullException(nameof(combine));
        }

        T result = seed;
        foreach (var task in tasks)
        {
            if (task == null)
            {
                throw new ArgumentException("Task collection contains a null task.", nameof(tasks));
            }

            T taskResult = await task.ConfigureAwait(false);
            result = combine(result, taskResult);
        }

        return result;
    }
}