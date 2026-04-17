public class Counter
{
    public int Value { get; private set; }

    public void Increment()
    {
        Value++;
    }

    public void Reset()
    {
        Value = 0;
    }

    public static Counter operator ++(Counter c)
    {
        c.Increment();
        return c;
    }
}