public static class ProductCatalog
{
            public IEnumerable<string> GetNewProducts(IEnumerable<string> current, IEnumerable<string> previous)
            {
                return current.Except(previous);
            }

            public IEnumerable<string> GetCommonProducts(IEnumerable<string> a, IEnumerable<string> b)
            {
                return Intersect(a, b);
            }

            public IEnumerable<string> GetAllUniqueProducts(IEnumerable<string> a, IEnumerable<string> b)
            {
                return Union(a, b);
            }
        }