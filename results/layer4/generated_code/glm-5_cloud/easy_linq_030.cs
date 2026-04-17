using System;
using System.Collections.Generic;
using System.Linq;

public static class Stats
{
    public static int CountWhere(IEnumerable<string> items, int minLength)
    {
        return items.Count(item => item.Length >= minLength);
    }
}