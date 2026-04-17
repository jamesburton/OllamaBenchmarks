using System;
using System.Threading.Tasks;

public static class TaskHelper
{
    public static Task<T> WrapException<T>(Exception ex)
    {
        return Task.FromException<T>(ex);
    }
}