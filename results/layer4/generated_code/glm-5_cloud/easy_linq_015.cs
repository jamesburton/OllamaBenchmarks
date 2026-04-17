using System;
using System.Collections.Generic;
using System.Linq;

public static class Zipper
{
    public static List<string> ZipCombine(IEnumerable<string> first, IEnumerable<string> second)
    {
        return first.Zip(second, (f, s) => $"{f}-{s}").ToList();
    }
}