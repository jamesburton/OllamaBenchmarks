using System;
using System.Collections.Generic;
using System.Linq;

public static class Stats
{
    public static double AverageLength(IEnumerable<string> items)
    {
        if (items == null)
        {
            throw new ArgumentNullException(nameof(items));
        }

        // Using LINQ Average with a selector to calculate the average length.
        // Note: This will throw an InvalidOperationException if the collection is empty.
        return items.Average(s => s.Length);
    }
}