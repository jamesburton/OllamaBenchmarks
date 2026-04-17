using System;
using System.Collections.Generic;
using System.Linq;

public static class Checker
{
    public static bool AllPositive(IEnumerable<int> numbers)
    {
        return numbers.All(n => n > 0);
    }
}