using System;
using System.Collections.Generic;

public static class StringHelper
{
    public static string Repeat(string s, int times)
    {
        if (times < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(times), "Times must be non-negative.");
        }

        // Using a collection expression to create a List<string> 
        // and passing it to string.Concat
        return string.Concat(times > 0 ? new List<string>(times) { s } : []);
    }
}