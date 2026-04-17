using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public static class Sequencer
{
    public static async Task<List<T>> RunSequentialAsync<T>(IEnumerable<Func<Task<T>>> factories)
    {
        var results = new List<T>();

        foreach (var factory in factories)
        {
            // Await each task sequentially to ensure they run one at a time
            T result = await factory();
            results.Add(result);
        }

        return results;
    }
}