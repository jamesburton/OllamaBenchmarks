using System;
using System.Collections.Generic;
using System.Linq;

public static class Mapper
{
    public static List<string> SelectTrimmed(IEnumerable<string> items)
    {
        return items.Select(item => item.Trim()).ToList();
    }
}