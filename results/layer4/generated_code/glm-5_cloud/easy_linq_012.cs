using System;
using System.Collections.Generic;
using System.Linq;

public static class Stats
{
    public static int Min(IEnumerable<int> numbers)
    {
        return numbers.Min();
    }

    public static int Max(IEnumerable<int> numbers)
    {
        return numbers.Max();
    }
}