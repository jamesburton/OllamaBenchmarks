using System;
using System.Collections.Generic;
using System.Linq;

public static class Mapper
{
    public static List<string> SelectIndexed(IEnumerable<string> items)
    {
        return items.Select((item, index) => $"{index}: {item}").ToList();
    }
}