using System.Collections.Generic;
using System.Threading.Tasks;

public static class AsyncMath
{
    public static async Task<int> SumAsync(IEnumerable<int> numbers)
    {
        int sum = 0;
        foreach (int number in numbers)
        {
            // Using await Task.FromResult to simulate an asynchronous operation for each number
            sum += await Task.FromResult(number);
        }
        return sum;
    }
}