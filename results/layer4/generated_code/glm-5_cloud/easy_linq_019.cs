using System;
using System.Collections.Generic;
using System.Linq;

public static class Filter
{
    public static List<int> WhereEven(IEnumerable<int> numbers)
    {
        return numbers.Where(n => n % 2 == 0).ToList();
    }
}