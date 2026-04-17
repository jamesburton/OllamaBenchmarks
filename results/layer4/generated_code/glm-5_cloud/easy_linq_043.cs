using System;
using System.Collections.Generic;
using System.Linq;

public static class Stats
{
    public static int MaxStringLength(IEnumerable<string> items)
    {
        return items.Max(s => s.Length);
    }
}