using System;
using System.Collections.Generic;
using System.Linq;

public static class Deduplicator
{
    public static List<string> UniqueStrings(IEnumerable<string> items)
    {
        return items.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }
}