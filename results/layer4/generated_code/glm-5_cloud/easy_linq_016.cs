using System;
using System.Collections.Generic;
using System.Linq;

public static class Combiner
{
    public static List<int> Concat(IEnumerable<int> first, IEnumerable<int> second)
    {
        return first.Concat(second).ToList();
    }
}