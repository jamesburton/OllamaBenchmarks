using System;

public static class Guard
{
    public static string ThrowIfNullOrEmpty(string? value, string paramName)
    {
        if (value is null)
        {
            throw new ArgumentNullException(paramName);
        }

        if (value.Length == 0)
        {
            throw new ArgumentException("The value cannot be an empty string.", paramName);
        }

        return value;
    }
}