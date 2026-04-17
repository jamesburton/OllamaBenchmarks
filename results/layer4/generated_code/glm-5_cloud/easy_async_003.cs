using System.Threading.Tasks;

public static class Waiter
{
    public static async Task WaitMilliseconds(int milliseconds)
    {
        await Task.Delay(milliseconds);
    }
}