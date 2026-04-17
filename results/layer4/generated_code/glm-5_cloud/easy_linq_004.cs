using System;
using System.Collections.Generic;
using System.Linq;

public static class Sorter
{
    public static List<string> OrderDescending(IEnumerable<string> items)
    {
        return items.OrderByDescending(item => item).ToList();
    }
}