static void Main(string[] args)
{
            var sales = new List<Sale>
            {
                new Sale { Product = "Laptop", Category = "Electronics", Amount = 999.99, DateOnly = new DateTime(2023, 1, 1) },
                new Sale { Product = "Phone", Category = "Electronics", Amount = 599.99, DateOnly = new DateTime(2023, 1, 2) },
                new Sale { Product = "Headphones", Category = "Electronics", Amount = 199.99, DateOnly = new DateTime(2023, 1, 3) },
                new Sale { Product = "Tablet", Category = "Electronics", Amount = 549.99, DateOnly = new DateTime(2023, 1, 4) },
                new Sale { Product = "Laptop", Category = "Electronics", Amount = 1999.99, DateOnly = new DateTime(2023, 1, 5) },
                new Sale { Product = "Phone", Category = "Electronics", Amount = 799.99, DateOnly = new DateTime(2023, 1, 6) },
            };

            var categoriesSummary = SalesAnalyzer.SummarizeByCategory(sales);
            foreach (var categorySummary in categoriesSummary)
            {
                Console.WriteLine($"Category: {categorySummary.Category} | TotalAmount: {categorySummary.TotalAmount:C} | AverageAmount: {categorySummary.AverageAmount:C} | Count: {categorySummary.Count}");
            }
        }