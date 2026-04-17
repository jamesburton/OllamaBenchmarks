using System.Threading.Tasks;

public static class TaskHelper
{
    public static Task<T> AsTask<T>(T value)
    {
        return Task.FromResult(value);
    }
}