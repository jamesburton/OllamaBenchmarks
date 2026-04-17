using System;
using System.Collections.Generic;
using System.Linq;

public static class Filter
{
    public static List<int> WhereGreaterThan(IEnumerable<int> numbers, int threshold)
    {
        return numbers.Where(n => n > threshold).ToList();
    }
}