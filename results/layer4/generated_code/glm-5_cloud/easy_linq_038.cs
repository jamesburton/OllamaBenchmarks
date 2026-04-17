using System;
using System.Collections.Generic;
using System.Linq;

public static class SetOps
{
    public static List<int> Intersection(IEnumerable<int> first, IEnumerable<int> second)
    {
        return first.Intersect(second).ToList();
    }
}