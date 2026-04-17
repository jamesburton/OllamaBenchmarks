using System;
using System.Threading.Tasks;

public static class TaskChain
{
    public static async Task<int> ChainAsync(int start)
    {
        // 1. Awaits Task.FromResult(start) to get the initial value
        int step1 = await Task.FromResult(start);

        // 2. Awaits a second task that adds 10
        int step2 = await Task.Run(() => step1 + 10);

        // 3. Awaits a third task that multiplies by 2
        int step3 = await Task.Run(() => step2 * 2);

        // 4. Returns the final result
        return step3;
    }
}