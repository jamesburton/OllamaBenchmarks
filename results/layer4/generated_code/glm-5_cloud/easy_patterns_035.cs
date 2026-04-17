using System;

public static class Validator
{
    public static bool IsValidAge(int? age)
    {
        // Using null pattern combined with range check.
        // The 'not null' pattern ensures age has a value.
        // The 'and' operator combines it with the range check (> 0 and < 150).
        return age is not null and > 0 and < 150;
    }
}