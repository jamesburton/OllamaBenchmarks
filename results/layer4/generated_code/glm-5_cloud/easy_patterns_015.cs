public static class PatternMatcher
{
    public static bool IsInRange(int value, int min, int max) =>
        value switch
        {
            _ when value >= min && value <= max => true,
            _ => false
        };
}