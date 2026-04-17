using System;
using System.Collections.Generic;
using System.Linq;

public static class Sorter
{
    public static List<int> OrderAscending(IEnumerable<int> numbers)
    {
        return numbers.OrderBy(n => n).ToList();
    }
}