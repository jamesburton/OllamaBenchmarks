using System;
using System.Collections.Generic;
using System.Linq;

public static class Checker
{
    public static bool AnyContains(IEnumerable<string> items, string substring)
    {
        return items.Any(item => item.Contains(substring));
    }
}