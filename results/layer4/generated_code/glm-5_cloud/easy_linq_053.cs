using System;
using System.Collections.Generic;
using System.Linq;

public static class Stats
{
    public static int Product(IEnumerable<int> numbers)
    {
        return numbers.Aggregate(1, (acc, n) => acc * n);
    }
}