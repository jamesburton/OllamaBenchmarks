using System;
using System.Collections.Generic;
using System.Linq;

public static class Checker
{
    public static bool AnyNegative(IEnumerable<int> numbers)
    {
        return numbers.Any(n => n < 0);
    }
}