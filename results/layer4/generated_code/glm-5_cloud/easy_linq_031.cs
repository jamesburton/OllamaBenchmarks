using System;
using System.Collections.Generic;
using System.Linq;

public static class Sorter
{
    public static List<int> SortDescending(IEnumerable<int> numbers)
    {
        return numbers.OrderByDescending(n => n).ToList();
    }
}