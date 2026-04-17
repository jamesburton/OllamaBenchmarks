public static class TypeChecker
{
    public static string Describe(object? obj)
    {
        return obj switch
        {
            null => "null",
            int i => $"integer: {i}",
            string s => $"string: {s}",
            _ => "other"
        };
    }
}