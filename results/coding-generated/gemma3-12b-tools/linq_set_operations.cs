public static class ProductCatalog
{
    public static IEnumerable<string> GetNewProducts(IEnumerable<string> current, IEnumerable<string> previous)
    {
        return current.Except(previous, StringComparer.Ordinal);
    }

    public static IEnumerable<string> GetCommonProducts(IEnumerable<string> a, IEnumerable<string> b)
    {
        return a.Intersect(b, StringComparer.Ordinal);
    }

    public static IEnumerable<string> GetAllUniqueProducts(IEnumerable<string> a, IEnumerable<string> b)
    {
        return a.Union(b, StringComparer.Ordinal);
    }
}