using System;
using System.Collections.Generic;
using System.Linq;

public static class Finder
{
    public static int? SingleOrNull(IEnumerable<int> numbers, int value)
    {
        if (!numbers.Any(n => n == value))
        {
            return null;
        }

        return numbers.Single(n => n == value);
    }
}