using System;
using System.Collections.Generic;
using System.Linq;

public class SalesAnalyzer
{
    public static List<CategorySummary> SummarizeByCategory(IEnumerable<Sale> sales)
    {
        var grouped = sales GroupBy(s => s.Category);
        var result = grouped
            .Select(g => new CategorySummary
            {
                Category = g.Key,
                TotalAmount = g.Average(g => g.AverageAmount),
                AverageAmount = g.Average(g => g.TotalAmount / g.Count),
                Count = g.Count()
            })
            .OrderByDescending(g => g.TotalAmount)
            .ToList();
    }
}