public record Name(string First, string Last)
{
    public string FullName() => $"{First} {Last}";
}

public static class NameHelper
{
    public static Name Swap(Name name) => name with { First = name.Last, Last = name.First };
}