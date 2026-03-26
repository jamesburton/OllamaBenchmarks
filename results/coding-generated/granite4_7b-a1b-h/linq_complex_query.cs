using System;
using System.Collections.Generic;
using System.Linq;

public record Sale(string Product, string Category, decimal Amount, DateTime Date);

public record CategorySummary(string Category, decimal TotalAmount, decimal AverageAmount, int Count);

static class SalesAnalyzer
{
    public static List<CategorySummary> SummarizeByCategory(IEnumerable<Sale> sales)
    {
        return sales.GroupBy(sale => sale.Category)
            .Select(group =>
                new CategorySummary(
                    group.Key,
                    group.Sum(sale => sale.Amount),
                    group.Average(sale => sale.Amount),
                    group.Count()))
            .OrderByDescending(summary => summary.TotalAmount)
            .ToList();
    }
}