using System;
using System.Collections.Generic;
using System.Linq;

public static class Mapper
{
    public static List<int> SelectAbsoluteValues(IEnumerable<int> numbers)
    {
        return numbers.Select(Math.Abs).ToList();
    }
}