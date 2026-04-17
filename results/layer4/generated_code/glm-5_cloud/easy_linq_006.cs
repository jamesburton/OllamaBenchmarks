using System;
using System.Collections.Generic;
using System.Linq;

public static class Finder
{
    public static string? Last(IEnumerable<string> items, string suffix)
    {
        return items.LastOrDefault(item => item.EndsWith(suffix));
    }
}