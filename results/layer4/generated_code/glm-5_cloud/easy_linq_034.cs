using System;
using System.Collections.Generic;
using System.Linq;

public static class Filter
{
    public static List<string> WhereNonEmpty(IEnumerable<string?> items)
    {
        return items.Where(item => !string.IsNullOrEmpty(item)).ToList()!;
    }
}