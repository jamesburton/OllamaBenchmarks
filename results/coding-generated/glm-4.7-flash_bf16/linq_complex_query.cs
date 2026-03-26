using System;
using System.Collections.Generic;
using System.Linq;

// Define the Sale record
public record Sale(string Product, string Category, decimal Amount, DateOnly Date);

// Define the CategorySummary record
public record CategorySummary(string Category, decimal TotalAmount, decimal AverageAmount, int Count);

// Define the static class with the method
public static class SalesAnalyzer
{
    public static List<CategorySummary> SummarizeByCategory(IEnumerable<Sale> sales)
    {
        return sales
            .GroupBy(s => s.Categoryigs)
            .Select(g => new CategorySummary(
                g.Key, 
                g.Sum(s => s.Amount), 
                g.Average(s => s.Amount), 
                g.Count()
            ))
            .OrderByDescending(s => s.TotalAmount)
            .ToList();
    }
}