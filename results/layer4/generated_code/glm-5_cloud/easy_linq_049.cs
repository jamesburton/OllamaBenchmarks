using System;
using System.Collections.Generic;
using System.Linq;

public static class Combiner
{
    public static List<string> ConcatDistinct(IEnumerable<string> first, IEnumerable<string> second)
    {
        return first.Concat(second).Distinct().ToList();
    }
}