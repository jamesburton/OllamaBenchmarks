public static class ProductCatalog
{
    public static IEnumerable<string> GetNewProducts(IEnumerable<string> current, IEnumerable<string> previous)
    {
        return System.Linq.Enumerable.Except(current, previous);
    }

    public static IEnumerable<string> GetCommonProducts(IEnumerable<string> a, IEnumerable<string> b)
    {
        return System.Linq.Enumerable.Intersect(a, b);
    }

    public static IEnumerable<string> GetAllUniqueProducts(IEnumerable<string> a, IEnumerable<string> b)
    {
        return System.Linq.Enumerable.Union(a, b);
    }
}