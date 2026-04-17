using System;
using System.Collections.Generic;
using System.Linq;

public static class Filter
{
    public static List<string> WhereStartsWith(IEnumerable<string> items, string prefix)
    {
        return items.Where(item => item.StartsWith(prefix)).ToList();
    }
}