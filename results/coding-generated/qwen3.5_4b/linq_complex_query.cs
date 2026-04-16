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