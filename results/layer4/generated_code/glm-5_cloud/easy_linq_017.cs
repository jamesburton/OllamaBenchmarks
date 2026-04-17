using System;
using System.Collections.Generic;
using System.Linq;

public static class Grouper
{
    public static Dictionary<char, int> CountByFirstLetter(IEnumerable<string> items)
    {
        return items
            .Where(s => !string.IsNullOrEmpty(s))
            .GroupBy(s => s[0])
            .ToDictionary(g => g.Key, g => g.Count());
    }
}