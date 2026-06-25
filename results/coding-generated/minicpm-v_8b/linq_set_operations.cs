using System;
using System.Collections.Generic;

public static class ProductCatalog {
    public static IEnumerable<string> GetNewProducts(IEnumerable<string> current, IEnumerable<string> previous) =>
        current.Except(previous);

    public static IEnumerable<string> GetCommonProducts(IEnumerable<string> a, IEnumerable<string> b) =>
        a.Intersect(b);

    public static IEnumerable<string> GetAllUniqueProducts(IEnumerable<string> a, IEnumerable<string> b) =>
        a.Union(b);
}