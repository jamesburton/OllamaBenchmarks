using System;
using System.Collections.Generic;
using System.Linq;

public class Sale
{
    public string Product { get; set; }
    public string Category { get; set; }
    public decimal Amount { get; set; }
    public DateOnly Date { get; set; }
}

public class CategorySummary
{
    public string Category { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AverageAmount { get; set; }
    public int Count { get; set; }
}

public static class SalesAnalyzer
{
    public static List<CategorySummary> SummarizeByCategory(IEnumerable<Sale> sales)
    {
        return sales
            .GroupBy(s => s.Category)
            .Select(group => new CategorySummary
            {
                Category = group.Key,
                TotalAmount = group.Sum(s => s.Amount),
                AverageAmount = group.Average(s => s.Amount),
                Count = group.Count()
            })
            .OrderByDescending(summary => summary.TotalAmount)
            .ToList();
    }
}