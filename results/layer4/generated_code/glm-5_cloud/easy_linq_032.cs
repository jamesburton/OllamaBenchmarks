using System;
using System.Collections.Generic;

public static class Aggregator
{
    public static string JoinWithComma(IEnumerable<string> items)
    {
        if (items == null)
        {
            throw new ArgumentNullException(nameof(items));
        }

        return string.Join(",", items);
    }
}