public static class NullHelper
{
    public static int GetLength(string? text)
    {
        return text?.Length ?? 0;
    }
}