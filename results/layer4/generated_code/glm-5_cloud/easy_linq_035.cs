using System;
using System.Collections.Generic;
using System.Linq;

public static class Grouper
{
    public static Dictionary<bool, List<int>> PartitionByEven(IEnumerable<int> numbers)
    {
        return numbers
            .GroupBy(n => n % 2 == 0)
            .ToDictionary(g => g.Key, g => g.ToList());
    }
}