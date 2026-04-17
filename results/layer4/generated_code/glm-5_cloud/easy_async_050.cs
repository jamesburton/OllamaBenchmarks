using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public static class AsyncMax
{
    public static async Task<T?> FindMaxAsync<T>(IEnumerable<Task<T>> tasks) where T : struct, IComparable<T>
    {
        // Await all tasks to complete. 
        // WhenAll returns the results in the same order as the input tasks.
        T[] results = await Task.WhenAll(tasks);

        // If the collection is empty, return null.
        if (results.Length == 0)
        {
            return null;
        }

        // Initialize max with the first element.
        T max = results[0];

        // Iterate through the rest of the results to find the maximum.
        for (int i = 1; i < results.Length; i++)
        {
            // Compare returns > 0 if the current instance is greater than the argument.
            if (results[i].CompareTo(max) > 0)
            {
                max = results[i];
            }
        }

        return max;
    }
}