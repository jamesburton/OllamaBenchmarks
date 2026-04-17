using System;

public static class Maybe
{
    public static TResult? Map<T, TResult>(T? value, Func<T, TResult> transform) where T : class where TResult : class
    {
        return value?.Let(transform);
    }
}

internal static class Extensions
{
    internal static TResult Let<T, TResult>(this T value, Func<T, TResult> transform)
    {
        return transform(value);
    }
}