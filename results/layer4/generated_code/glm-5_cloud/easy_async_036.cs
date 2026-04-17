using System;
using System.Threading.Tasks;

public static class ParallelMapper
{
    public static async Task<int[]> DoubleAllAsync(int[] numbers)
    {
        if (numbers == null)
        {
            throw new ArgumentNullException(nameof(numbers));
        }

        var tasks = new Task<int>[numbers.Length];

        for (int i = 0; i < numbers.Length; i++)
        {
            int index = i; // Capture index to avoid closure modification issues
            tasks[i] = Task.Run(() => numbers[index] * 2);
        }

        return await Task.WhenAll(tasks);
    }
}