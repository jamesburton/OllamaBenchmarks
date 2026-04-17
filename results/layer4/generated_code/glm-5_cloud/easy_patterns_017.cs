public static class NullCoalesceHelper
{
    public static int SumOrZero(int? a, int? b)
    {
        return (a ?? 0) + (b ?? 0);
    }
}