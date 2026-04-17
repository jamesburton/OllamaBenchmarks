using System;
using System.Collections.Generic;
using System.Linq;

public static class Mapper
{
    public static List<string> SelectUpperCase(IEnumerable<string> items)
    {
        return items.Select(s => s.ToUpper()).ToList();
    }
}