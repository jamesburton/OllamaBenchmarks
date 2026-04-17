using System;
using System.Threading.Tasks;

public class AsyncEventCounter
{
    private int _count;

    public async Task FireEventAsync()
    {
        await Task.Yield();
        _count++;
    }

    public int Count
    {
        get { return _count; }
    }
}