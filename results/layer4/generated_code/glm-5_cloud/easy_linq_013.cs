using System.Collections.Generic;
using System.Linq;

public static class Deduplicator
{
    public static List<int> Unique(IEnumerable<int> numbers)
    {
        return numbers.Distinct().ToList();
    }
}