using System;

public static class RomanNumerals
{
    public static string ToRoman(int number)
    {
        if (number < 1 || number > 3999)
        {
            throw new ArgumentOutOfRangeException(nameof(number), "Input must be between 1 and 3999.");
        }

        // Define the mapping of values to Roman numeral symbols.
        // Includes subtractive combinations (e.g., 4 is "IV", 9 is "IX").
        int[] values = { 1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1 };
        string[] symbols = { "M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX", "V", "IV", "I" };

        var result = new System.Text.StringBuilder();

        // Iterate through the values, appending the corresponding symbols
        // while reducing the number until it reaches zero.
        for (int i = 0; i < values.Length; i++)
        {
            while (number >= values[i])
            {
                result.Append(symbols[i]);
                number -= values[i];
            }
        }

        return result.ToString();
    }
}