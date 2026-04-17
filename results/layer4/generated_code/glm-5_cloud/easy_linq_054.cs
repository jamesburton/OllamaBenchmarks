using System;
using System.Collections.Generic;
using System.Linq;

public static class Filter
{
    public static List<string> WhereContains(IEnumerable<string> items, string keyword)
    {
        return items.Where(item => item.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
    }
}