using System;
using System.Collections.Generic;
using System.Linq;

public static class Pager
{
    public static List<T> TakePage<T>(IEnumerable<T> items, int skip, int take)
    {
        return items.Skip(skip).Take(take).ToList();
    }
}