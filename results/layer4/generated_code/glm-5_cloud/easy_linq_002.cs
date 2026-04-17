using System;
using System.Collections.Generic;
using System.Linq;

public static class Mapper
{
    public static List<int> SelectLengths(IEnumerable<string> items)
    {
        return items.Select(item => item.Length).ToList();
    }
}