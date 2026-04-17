public static class NullAssign
{
    public static string EnsureValue(ref string? field, string defaultValue)
    {
        field ??= defaultValue;
        return field;
    }
}