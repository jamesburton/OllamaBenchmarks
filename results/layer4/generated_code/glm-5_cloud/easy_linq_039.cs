using System;
using System.Collections.Generic;
using System.Linq;

public static class SetOps
{
    public static List<int> Difference(IEnumerable<int> first, IEnumerable<int> second)
    {
        return first.Except(second).ToList();
    }
}