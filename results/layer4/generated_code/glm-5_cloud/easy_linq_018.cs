using System;
using System.Collections.Generic;
using System.Linq;

public static class Flattener
{
    public static List<int> Flatten(IEnumerable<IEnumerable<int>> nested)
    {
        return nested.SelectMany(x => x).ToList();
    }
}