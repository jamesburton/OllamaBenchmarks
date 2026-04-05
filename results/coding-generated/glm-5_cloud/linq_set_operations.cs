public static class ProductCatalog
{
    public static System.Collections.Generic.IEnumerable<string> GetNewProducts(
        System.Collections.Generic.IEnumerable<string> current,
        System.Collections.Generic.IEnumerable<string> previous)
    {
        return System.Linq.Enumerable.Except(current, previous);
    }

    public static System.Collections.Generic.IEnumerable<string> GetCommonProducts(
        System.Collections.Generic.IEnumerable<string> a,
        System.Collections.Generic.IEnumerable<string> b)
    {
        return System.Linq.Enumerable.Intersect(a, b);
    }

    public static System.Collections.Generic.IEnumerable<string> GetAllUniqueProducts(
        System.Collections.Generic.IEnumerable<string> a,
        System.Collections.Generic.IEnumerable<string> b)
    {
        return System.Linq.Enumerable.Union(a, b);
    }
}