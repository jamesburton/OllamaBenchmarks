using System;
using System.Linq;
using System.Collections.Generic;

public record Sale(string Product, string Category, decimal Amount, DateOnly Date);

public record CategorySummary(string Category, decimal TotalAmount, decimal AverageAmount, int Count);

public static class SalesAnalyzer
{
    public static List<CategorySummary> SummarizeByCategory(IEnumerable<Sale> sales)
    {
        return sales
            .GroupBy(s => s.Category)
            .Select(g => new CategorySummary(
                g.Key,
                g.Sum(s => s.Amount),
                g.Average(s => s.Amount),
                g.Count()
            ))
            .OrderByDescending(x => x.TotalAmount)
            .ToList();
    }
}