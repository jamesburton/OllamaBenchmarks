record Sale(string Product, string Category, decimal Amount, DateOnly Date);
record CategorySummary(string Category, decimal TotalAmount, decimal AverageAmount, int Count);

public static class SalesAnalyzer
{
    public static List<CategorySummary> SummarizeByCategory(IEnumerable<Sale> sales)
    {
        return sales
            .GroupBy(s => s.Category)
            .Select(g => new CategorySummary
            {
                Category = g.Key,
                TotalAmount = g.Sum(s => s.Amount),
                AverageAmount = g.Average(s => s.Amount),
                Count = g.Count()
            })
            .OrderByDescending(s => s.TotalAmount)
            .ToList();
    }
}