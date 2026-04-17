using System.Threading.Tasks;

public class Counter
{
    private int _count = 0;

    public async Task IncrementAsync()
    {
        await Task.Yield();
        _count++;
    }

    public int GetCount()
    {
        return _count;
    }
}