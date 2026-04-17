using System;
using System.Collections.Generic;
using System.Linq;

public static class Reverser
{
    public static List Reverse(IEnumerable items)
    {
        return items.Reverse().ToList();
    }
}