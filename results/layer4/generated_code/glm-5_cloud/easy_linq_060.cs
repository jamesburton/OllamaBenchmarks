using System;
using System.Collections.Generic;
using System.Linq;

public static class Pager
{
    public static List<T> TakeLast<T>(IEnumerable<T> items, int count)
    {
        return items.TakeLast(count).ToList();
    }
}