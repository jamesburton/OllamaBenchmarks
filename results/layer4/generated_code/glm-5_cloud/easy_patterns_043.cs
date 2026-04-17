using System;

public static class PatternParser
{
    public static (bool Success, int Value) TryParseInt(string input)
    {
        if (int.TryParse(input, out int result))
        {
            return (true, result);
        }

        return (false, 0);
    }
}