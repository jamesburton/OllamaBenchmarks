using System;
using System.Collections.Generic;

public class Sale {
    public string Product { get; set; }
    public string Category { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }

    public Sale(string product, string category, decimal amount, DateTime date) {
        this.Product = product;
        this.Category = category;
        this.Amount = amount;
        this.Date = date;
    }
}

public class CategorySummary {
    public string Category { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AverageAmount { get; set; }
    public int Count { get; set; }

    public CategorySummary(string category, decimal totalAmount, decimal averageAmount, int count) {
        this.Category = category;
        this.TotalAmount = totalAmount;
        this.AverageAmount = averageAmount;
        this.Count = count;
    }
}

public static class SalesAnalyzer {
    public static List<CategorySummary> SummarizeByCategory(IEnumerable<Sale> sales) {
        return sales
            .GroupBy(s => s.Category)
            .Select(g => new CategorySummary(
                g.Key,
                g.Sum(s => s.Amount),
                g.Average(s => s.Amount),
                g.Count()
            ))
            .OrderByDescending(c => c.TotalAmount)
            .ToList();
    }
}