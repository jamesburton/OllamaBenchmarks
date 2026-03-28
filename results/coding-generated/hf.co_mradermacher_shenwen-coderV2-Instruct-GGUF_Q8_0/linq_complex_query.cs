using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Numerics;
using System;
using System.Collections.Generic;

class SalesAnalyzer
{
    public static List<CategorySummary> SummarizeByCategory(IEnumerable<Sale> sales)
    {
        return sales.GroupBy(s => s.Category)
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