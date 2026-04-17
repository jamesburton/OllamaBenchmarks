using System;
using System.Collections.Generic;
using System.Linq;

public static class Mapper
{
    public static List<(string Name, int Length)> SelectNameLength(IEnumerable<string> items)
    {
        return items.Select(item => (Name: item, Length: item.Length)).ToList();
    }
}