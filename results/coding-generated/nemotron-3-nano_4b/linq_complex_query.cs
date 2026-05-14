using System;
using System.Collections.Generic;
using System.Linq;

public class Sale
{
    public string Product { get; }
    public string Category { get; }
    public decimal Amount { get; }
    public DateOnly Date { get; }

    public Sale(string product, string category, decimal amount, DateOnly date)
    {
        Product = product;
        Category = category;
        Amount = amount;
        Date = date;
    }
}

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
            .OrderByDescending(ts => ts.TotalAmount)
            .ToList();
    }
}