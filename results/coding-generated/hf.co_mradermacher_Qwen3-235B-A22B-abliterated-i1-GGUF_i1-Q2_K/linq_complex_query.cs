using System;
using System.Collections.Generic;
using System.Linq;

record Sale(string Product, string Category, decimal Amount, DateOnly Date);

record CategorySummary(string Category, decimal TotalAmount, decimal AverageAmount, int Count);

static class SalesAnalyzer
{
    public static List<CategorySummary> SummarizeByCategory(IEnumerable<Sale> sales)
    {
        return sales.GroupBy(s => s.Category)
            .Select(g => new CategorySummary(
                g.Key,
                g.Sum(s => s.Amount),
                g.Average(s => s.Amount),
                g.Count()))
            .OrderByDescending(c => c.TotalAmount)
            .ToList();
    }
}