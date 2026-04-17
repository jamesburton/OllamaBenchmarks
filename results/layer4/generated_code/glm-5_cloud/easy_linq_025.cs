using System;
using System.Collections.Generic;
using System.Linq;

public static class Pager
{
    public static List<T> SkipFirst<T>(IEnumerable<T> items, int count)
    {
        return items.Skip(count).ToList();
    }
}