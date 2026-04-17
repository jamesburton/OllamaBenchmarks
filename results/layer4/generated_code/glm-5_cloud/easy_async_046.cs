using System.Threading;
using System.Threading.Tasks;

public static class CancellableLoop
{
    public static async Task<int> CountUntilCancelledAsync(CancellationToken token, int maxCount)
    {
        int count = 0;

        while (count < maxCount)
        {
            if (token.IsCancellationRequested)
            {
                break;
            }

            count++;
            await Task.Yield();
        }

        return count;
    }
}