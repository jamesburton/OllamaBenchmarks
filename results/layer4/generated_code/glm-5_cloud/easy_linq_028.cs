using System;
using System.Collections.Generic;
using System.Linq;

public static class Finder
{
    public static int? FindFirst(IEnumerable<int> numbers, int threshold)
    {
        return numbers
            .Where(n => n > threshold)
            .Cast<int?>()
            .FirstOrDefault();
    }
}