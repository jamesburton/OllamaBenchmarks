using System;
using System.Collections.Generic;
using System.Linq;

public static class Finder
{
    public static string? First(IEnumerable<string> items, string prefix)
    {
        return items.FirstOrDefault(item => item.StartsWith(prefix));
    }
}