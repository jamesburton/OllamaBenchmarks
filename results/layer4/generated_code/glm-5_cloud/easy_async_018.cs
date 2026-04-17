using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public static class Throttler
{
    public static async Task<List<T>> RunBatchedAsync<T>(IEnumerable<Func<Task<T>>> factories, int batchSize)
    {
        if (factories == null)
        {
            throw new ArgumentNullException(nameof(factories));
        }

        if (batchSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(batchSize), "Batch size must be greater than zero.");
        }

        var results = new List<T>();
        var factoryList = factories.ToList();

        for (int i = 0; i < factoryList.Count; i += batchSize)
        {
            // Create the batch by skipping completed items and taking the next 'batchSize'
            var batchFactories = factoryList.Skip(i).Take(batchSize);

            // Execute all tasks in the batch concurrently
            var batchTasks = batchFactories.Select(f => f()).ToList();

            // Wait for all tasks in the batch to complete
            var batchResults = await Task.WhenAll(batchTasks).ConfigureAwait(false);

            // Add results to maintain order
            results.AddRange(batchResults);
        }

        return results;
    }
}