using System;
using System.Collections.Generic;
using System.Linq;

public record Sale(string Product, string Category, decimal Amount, DateOnly Date);

public record CategorySummary(string Category, decimal TotalAmount, decimal AverageAmount, int Count);

public static class SalesAnalyzer
{
    public static List<CategorySummary> SummarizeByCategory(IEnumerable<Sale> sales)
    {
        return sales.GroupBy(s => s.Category)
                .Select(group => new CategorySummary(
                    group.Key,
                    group.Sum(s => s.Amount),
                    (decimal)group.Average(s => s.Amount),
                    group.Count()
                ))
                .OrderByDescending(summary => summary.TotalAmount);
    }
}