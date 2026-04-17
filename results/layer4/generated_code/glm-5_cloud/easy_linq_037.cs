using System;
using System.Collections.Generic;
using System.Linq;

public static class Finder
{
    public static int IndexOf(IEnumerable<string> items, string target)
    {
        return items.Select((item, index) => new { Item = item, Index = index })
                    .Where(x => x.Item == target)
                    .Select(x => x.Index)
                    .FirstOrDefault(-1);
    }
}