using System;
using System.Collections.Generic;
using System.Linq;

public static class Counter
{
    public static int CountLongerThan(IEnumerable<string> items, int length)
    {
        return items.Count(item => item.Length > length);
    }
}