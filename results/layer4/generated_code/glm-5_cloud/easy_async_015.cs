using System.Threading.Tasks;

public static class ValueTaskHelper
{
    public static ValueTask<int> GetCachedOrComputeAsync(bool cached, int value)
    {
        if (cached)
        {
            return ValueTask.FromResult(value);
        }

        return new ValueTask<int>(Task.FromResult(value * 2));
    }
}