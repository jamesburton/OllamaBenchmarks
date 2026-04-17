using System;

public static class Validator
{
    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return false;
        }

        // Find the position of the '@' character
        int atIndex = email.IndexOf('@');

        // Check if there is exactly one '@'
        // IndexOf returns -1 if not found, or the index of the first occurrence.
        // LastIndexOf returns the index of the last occurrence.
        // If they are not equal, there is more than one '@'.
        if (atIndex == -1 || email.LastIndexOf('@') != atIndex)
        {
            return false;
        }

        // Check if there is at least one '.' after the '@'
        int dotIndexAfterAt = email.IndexOf('.', atIndex + 1);

        return dotIndexAfterAt != -1;
    }
}