public record Range(int Min, int Max)
{
    public bool Contains(int value) => value >= Min && value <= Max;
}

public static class RangeHelper
{
    public static Range Merge(Range a, Range b)
    {
        int min = a.Min < b.Min ? a.Min : b.Min;
        int max = a.Max > b.Max ? a.Max : b.Max;
        return new Range(min, max);
    }
}