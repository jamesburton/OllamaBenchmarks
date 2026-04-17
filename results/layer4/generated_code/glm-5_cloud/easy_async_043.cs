using System.Collections.Generic;
using System.Threading.Tasks;

public static class AsyncQueue
{
    public static async Task<Queue<T>> FillQueueAsync<T>(IEnumerable<Task<T>> tasks)
    {
        var queue = new Queue<T>();

        foreach (var task in tasks)
        {
            var result = await task;
            queue.Enqueue(result);
        }

        return queue;
    }
}