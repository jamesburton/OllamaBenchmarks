public record Pair<T>(T First, T Second);

public static class PairHelper
{
    public static Pair<T> Swap<T>(Pair<T> pair) => new Pair<T>(pair.Second, pair.First);
}