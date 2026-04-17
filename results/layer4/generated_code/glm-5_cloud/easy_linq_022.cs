using System;
using System.Collections.Generic;
using System.Linq;

public static class Sorter
{
    public static List<string> OrderByLength(IEnumerable<string> items)
    {
        return items.OrderBy(s => s.Length).ToList();
    }
}