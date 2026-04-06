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

public static class ProductCatalog
    {
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

public static class ProductCatalog
        {
            public static System.Collections.Generic.IEnumerable<string> GetNewProducts(System.Collections.Generic.IEnumerable<string> current, System.Collections.Generic.IEnumerable<string> previous)
            {
                return current.Except(previous);
            }
            // ...
        }