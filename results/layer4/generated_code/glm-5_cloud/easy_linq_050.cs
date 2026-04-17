using System;
using System.Collections.Generic;
using System.Linq;

public static class Converter
{
    public static Dictionary<string, int> ToWordLengthMap(IEnumerable<string> items)
    {
        return items.ToDictionary(item => item, item => item.Length);
    }
}