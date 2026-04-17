using System.Threading.Tasks;

public static class Runner
{
    public static async Task<(T1 First, T2 Second)> RunBothAsync<T1, T2>(Task<T1> task1, Task<T2> task2)
    {
        // Start both tasks concurrently by awaiting them.
        // Using Task.WhenAll ensures that if one task throws an exception,
        // we still wait for the other to complete, and it allows concurrent execution.
        await Task.WhenAll(task1, task2);

        // At this point, both tasks are guaranteed to be completed.
        // We can safely access the Result property (or await again) to retrieve values.
        return (task1.Result, task2.Result);
    }
}