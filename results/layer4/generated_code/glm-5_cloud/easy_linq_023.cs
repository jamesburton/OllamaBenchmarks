using System;
using System.Collections.Generic;
using System.Linq;

public static class Stats
{
    public static int SumPositives(IEnumerable<int> numbers)
    {
        return numbers.Where(n => n > 0).Sum();
    }
}