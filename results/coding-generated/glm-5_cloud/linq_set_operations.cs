public static IEnumerable<string> GetNewProducts(IEnumerable<string> current, IEnumerable<string> previous)
    {
        return current.Except(previous);
    }

    public static IEnumerable<string> GetCommonProducts(IEnumerable<string> a, IEnumerable<string> b)
    {
        return a.Intersect(b);
    }

    public static IEnumerable<string> GetAllUniqueProducts(IEnumerable<string> a, IEnumerable<string> b)
    {
        return a.Union(b);
    }
}