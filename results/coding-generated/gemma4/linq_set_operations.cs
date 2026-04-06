using System.Collections.Generic;
using System.Linq;

public static class ProductCatalog
{
    /// <summary>
    /// Returns products in 'current' that are NOT present in 'previous'.
    /// </summary>
    public static IEnumerable<string> GetNewProducts(IEnumerable<string> current, IEnumerable<string> previous)
    {
        return current.Except(previous);
    }

    /// <summary>
    /// Returns products present in BOTH sequences 'a' and 'b'.
    /// </summary>
    public static IEnumerable<string> GetCommonProducts(IEnumerable<string> a, IEnumerable<string> b)
    {
        return a.Intersect(b);
    }

    /// <summary>
    /// Returns all distinct products from both sequences 'a' and 'b' combined.
    /// </summary>
    public static IEnumerable<string> GetAllUniqueProducts(IEnumerable<string> a, IEnumerable<string> b)
    {
        return a.Union(b);
    }
}