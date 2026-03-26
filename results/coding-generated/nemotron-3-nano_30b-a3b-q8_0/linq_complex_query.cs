using System;
using System.Collections.Generic;
using System.Linq;

public record Sale(string Product, string Category, decimal Amount, DateOnly Date);
public record CategorySummary(string Category, decimal TotalAmount, decimal AverageAmount, int Count);

public static class SalesAnalyzer
{
    public static List<CategorySummary> SummarizeByCategory(IEnumerable<Sale> sales)
    {
        return sales
            .GroupBy(s => s.Category)
            .Select(g => new CategorySummary
            {
                Category = g.Key,
                TotalAmount = g.Sum(x => x.Amount),
                AverageAmount = g.Average(x => x.Amount),
                Count = g.Count()
            })
            .OrderByDescending(c => c.TotalAmount)
            .ToList();
    }
}