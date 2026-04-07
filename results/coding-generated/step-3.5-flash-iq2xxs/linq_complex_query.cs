using System;
using System.Collections.Generic;
using System.Linq;

public record Sale(string Product, string Category, decimal Amount, DateOnly Date);
public record CategorySummary(string Category, decimal TotalAmount, decimal AverageAmount, int Count);

public static class SalesAnalyzer
{
    public static List<CategorySummary> SummarizeByCategory(IEnumerable<Sale> sales)
    {
        // implementation
    }
}

But note: the Sale record has a DateOnly property. We are using DateOnly from System. So we need to include using System; for DateOnly? Actually DateOnly is in System. So we need to use that.

But the problem says "DateOnly Date" in the Sale record. So we'll have a field of type DateOnly.

We'll write:

public record Sale(string Product, string Category, decimal Amount, DateOnly Date);

But note: the problem says "A Sale record: (string Product, string Category, decimal Amount, DateOnly Date)" so we are using positional records.

Now, the method:

We'll write:

public static List<CategorySummary> SummarizeByCategory(IEnumerable<Sale> sales)
{
    var summaries = sales
        .GroupBy(s => s.Category)
        .Select(g => new CategorySummary(
            g.Key,
            g.Sum(s => s.Amount),
            g.Average(s => s.Amount),
            g.Count()))
        .OrderByDescending(cs => cs.TotalAmount)
        .ToList();
    return summaries;
}

But wait: the problem says "using LINQ method syntax (GroupBy, Select, OrderByDescending)". We are using GroupBy, then Select, then OrderByDescending. That's fine.

But note: the OrderByDescending is applied to the entire sequence of CategorySummary objects, not within the group. That's correct.

However, we are ordering after the Select, which is what we want.

But we must ensure that the method returns a List<CategorySummary>. We are calling .ToList() at the end.

Now, we need to make sure the code is valid. We are using DateOnly, so we need to include the namespace for DateOnly? DateOnly is in System. So we have using System; at the top.

But we also need to use LINQ, so we need using System.Linq.

We'll write:

using System;
using System.Collections.Generic;
using System.Linq;

Then the records and the class.

But note: the problem says "Return only valid C# code in a single file." So we must output a complete file.

We'll write:

// A single file containing the required code.

We'll not include any extra comments unless necessary. But the problem says "Return only valid C# code", so we can include the code without extra text? Actually, we are allowed to include the code only. But we can include a brief comment? The problem says "Return only valid C# code in a single file." So we should not include any extra text. We'll just output the code.

But the user might expect the code to be in a single file, so we'll output the entire content of a .cs file.

We'll write:

using System;
using System.Collections.Generic;
using System.Linq;

public record Sale(string Product, string Category, decimal Amount, DateOnly Date);

public record CategorySummary(string Category, decimal TotalAmount, decimal AverageAmount, int Count);

public static class SalesAnalyzer
{
    public static List<CategorySummary> SummarizeByCategory(IEnumerable<Sale> sales)
    {
        var summaries = sales
            .GroupBy(s => s.Category)
            .Select(g => new CategorySummary(
                g.Key,
                g.Sum(s => s.Amount),
                g.Average(s => s.Amount),
                g.Count()))
            .OrderByDescending(cs => cs.TotalAmount)
            .ToList();
        return summaries;
    }
}

But wait: the problem says "A Sale record: (string Product, string Category, decimal Amount, DateOnly Date)" and we have that.

But note: the method returns List<CategorySummary>. We are returning a List<CategorySummary> that is ordered by TotalAmount descending.

But there is a potential issue: the Average method returns a decimal? Actually, the Average method for a sequence of decimal returns decimal. But if the group is empty, but groups are not empty. So it's safe.

But we have to be careful: the Average method for an empty group would throw an exception, but GroupBy ensures each group has at least one element. So it's safe.

Now, we need to make sure the code compiles. Let's check:

- We are using DateOnly, which is in System. So we have using System; at the top. That's fine.

- We are using GroupBy, Select, OrderByDescending, Sum, Average, Count. All are extension methods from LINQ.

- We are returning a List<CategorySummary>. The method is static.

One more thing: the problem says "Orders by TotalAmount descending". We are ordering the entire list of CategorySummary by