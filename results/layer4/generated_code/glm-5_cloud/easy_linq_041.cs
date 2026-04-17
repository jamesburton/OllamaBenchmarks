using System;
using System.Collections.Generic;
using System.Linq;

public static class Checker
{
    public static bool AllNonEmpty(IEnumerable<string> items)
    {
        return items.All(s => !string.IsNullOrEmpty(s));
    }
}