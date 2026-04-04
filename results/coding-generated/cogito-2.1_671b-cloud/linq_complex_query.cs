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
            .GroupBy(sale => sale.Category)
            .Select(group => new CategorySummary(
                Category: group.Key,
                TotalAmount: group.Sum(sale => sale.Amount),
                AverageAmount: group.Average(sale => sale.Amount),
                Count: group.Count()
            ))
            .OrderByDescending(summary => summary.TotalAmount)
            .ToList();
    }
}