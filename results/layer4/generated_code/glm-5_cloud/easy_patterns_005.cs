public static class NullHelper
{
    public static string GetValueOrDefault(string? input, string defaultValue)
    {
        return input ?? defaultValue;
    }
}