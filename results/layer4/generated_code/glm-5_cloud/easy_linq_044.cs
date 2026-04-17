using System;
using System.Collections.Generic;
using System.Linq;

public static class Grouper
{
    public static Dictionary<int, List<string>> GroupByLength(IEnumerable<string> items)
    {
        return items.GroupBy(s => s.Length)
                    .ToDictionary(g => g.Key, g => g.ToList());
    }
}