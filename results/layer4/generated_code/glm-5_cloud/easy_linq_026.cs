using System.Collections.Generic;
using System.Linq;

public static class Checker
{
    public static bool ContainsValue(IEnumerable<int> numbers, int value)
    {
        return numbers.Any(n => n == value);
    }
}