using System;

public static class BinaryConverter
{
    public static string ToBinary(int n)
    {
        if (n < 0)
        {
            throw new ArgumentException("Input must be a non-negative integer.", nameof(n));
        }

        // Convert.ToInt32 with base 2 handles the conversion logic.
        // We use Convert.ToString to get the binary representation.
        return Convert.ToString(n, 2);
    }

    public static int FromBinary(string binary)
    {
        if (binary == null)
        {
            throw new ArgumentNullException(nameof(binary), "Input string cannot be null.");
        }

        // Check if the string is empty or contains characters other than '0' or '1'
        foreach (char c in binary)
        {
            if (c != '0' && c != '1')
            {
                throw new ArgumentException("Input string must contain only binary characters ('0' or '1').", nameof(binary));
            }
        }

        // Handle empty string edge case (often treated as 0 in custom logic, but Convert.ToInt32 throws FormatException)
        if (binary.Length == 0)
        {
            return 0;
        }

        return Convert.ToInt32(binary, 2);
    }
}