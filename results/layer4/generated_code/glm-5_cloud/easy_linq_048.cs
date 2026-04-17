using System;
using System.Collections.Generic;
using System.Linq;

public static class Stats
{
    public static int SumOfSquares(IEnumerable<int> numbers)
    {
        return numbers.Select(n => n * n).Sum();
    }
}