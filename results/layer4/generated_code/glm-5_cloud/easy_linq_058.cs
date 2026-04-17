using System;
using System.Collections.Generic;
using System.Linq;

public static class Mapper
{
    public static List<double> SelectSqrt(IEnumerable<int> numbers)
    {
        return numbers
            .Where(n => n >= 0)
            .Select(Math.Sqrt)
            .ToList();
    }
}