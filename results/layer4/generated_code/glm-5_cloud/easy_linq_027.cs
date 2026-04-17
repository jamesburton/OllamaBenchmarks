using System;
using System.Collections.Generic;
using System.Linq;

public static class Mapper
{
    public static List<int> SelectSquares(IEnumerable<int> numbers)
    {
        return numbers.Select(n => n * n).ToList();
    }
}