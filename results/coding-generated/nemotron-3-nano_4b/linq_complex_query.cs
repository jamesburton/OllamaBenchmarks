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
            .GroupBy(s => s.Category)                                   // group by Category
            .Select(g => new CategorySummary(
                g.Key,                                                    // Category
                g.Sum(s => s.Amount),                                     // TotalAmount
                g.Average(),                                             // AverageAmount
                g.Count()))                                                // Count
            .OrderByDescending(c => c.TotalAmount)                     // order by TotalAmount descending
            .ToList();                                                 // materialize as List<CategorySummary>
    }
}