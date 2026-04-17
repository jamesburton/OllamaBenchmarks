using System;

public static class DeconstHelper
{
    public static void Deconstruct(string fullName, out string first, out string last)
    {
        int spaceIndex = fullName.IndexOf(' ');
        first = fullName.Substring(0, spaceIndex);
        last = fullName.Substring(spaceIndex + 1);
    }
}