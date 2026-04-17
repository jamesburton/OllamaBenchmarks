using System;
using System.Collections.Generic;
using System.Linq;

public static class Finder
{
    public static string? FindLongest(IEnumerable<string> items)
    {
        return items.OrderByDescending(x => x.Length).FirstOrDefault();
    }
}