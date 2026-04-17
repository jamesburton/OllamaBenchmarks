using System;

public static class ChainNullable
{
    public static string? GetNestedValue(string? input)
    {
        // Use null-conditional operator to call ToUpper if input is not null
        // Use null-coalescing operator to provide an empty string fallback for the conditional logic check
        // (though the final return handles nulls implicitly).
        // We capture the uppercased value.
        string? upper = input?.ToUpper();

        // If upper is null, return null.
        // If upper has length >= 3, return the substring.
        // Otherwise, return the whole upper value.
        return upper?.Length >= 3 
            ? upper.Substring(0, 3) 
            : upper;
    }
}