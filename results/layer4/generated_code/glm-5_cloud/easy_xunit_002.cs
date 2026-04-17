using System;

public static class StringHelper
{
    public static bool IsPalindrome(string s)
    {
        if (string.IsNullOrEmpty(s))
        {
            return true;
        }

        int start = 0;
        int end = s.Length - 1;

        while (start < end)
        {
            // Compare characters case-insensitively
            if (char.ToUpper(s[start]) != char.ToUpper(s[end]))
            {
                return false;
            }
            start++;
            end--;
        }

        return true;
    }
}