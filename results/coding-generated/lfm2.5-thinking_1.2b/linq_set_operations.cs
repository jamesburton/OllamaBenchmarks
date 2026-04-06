public class ProductCatalog
{
    public static IEnumerable<string> GetNewProducts(IEnumerable<string> current, IEnumerable<string> previous)
    {
        return current.Where(x => !previous.Contains(x)).ToList();
    }

    public static IEnumerable<string> GetCommonProducts(IEnumerable<string> a, IEnumerable<string> b)
    {
        return a.Intersect(b).ToList();
    }

    public static IEnumerable<string> GetAllUniqueProducts(IEnumerable<string> a, IEnumerable<string> b)
    {
        return a.Union(b).ToList();
    }
}