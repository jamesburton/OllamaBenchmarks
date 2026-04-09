), ONLY valid C# code.
    *   Constraints: No namespace, no using statements.
    *   Class Name: `SurveyAnalyzer` (static class).
    *   Method Name: `ComputeStats` (static method).
    *   Method Signature: `(double Mean, double Median, double Min, double Max) ComputeStats(IEnumerable<double> values)`.
    *   Logic Requirements:
        *   Throw `ArgumentException` if input is empty.
        *   Calculate Mean using LINQ `Average()`.
        *   Calculate Min using LINQ `Min()`.
        *   Calculate Max using LINQ `Max()`.
        *   Calculate Median: Sort values, odd count -> middle element, even count -> average of two middle elements.
        *   Return type: Named tuple `(double Mean, double Median, double Min, double Max)`.
    *   Testing Context: xUnit v3, AwesomeAssertions, NSubstitute (implies standard C# features needed).
    *   Specific Syntax Constraint: "Use positional record syntax when the prompt specifies records." -> The return type is a named tuple. In C#, named tuples are often represented as `var result = (Mean, Median, Min, Max);` or using `record`. However, the prompt asks for `(double Mean, double Median, double Min, double Max)`. This looks like a named tuple syntax.
    *   Constraint Check: "Do NOT wrap code in a namespace. Do NOT add using statements."

2.  **Drafting the Code:**

    *   Class definition: `public static class SurveyAnalyzer` (or just `static class SurveyAnalyzer`). The prompt says "A static class SurveyAnalyzer". Usually, public is implied for external usage, but I'll stick to minimal visibility or default if not specified. Let's make it `public` as it's a utility class likely meant to be used. Wait, the prompt says "Ensure all public types and methods exactly match the names specified in the prompt." It doesn't explicitly say `public`, but standard practice for classes exposed like this is `public`. However, to be safe regarding "exactly match", I will use `public static class SurveyAnalyzer`.
    *   Method signature: `public static (double Mean, double Median, double Min, double Max) ComputeStats(IEnumerable<double> values)`
    *   Empty check: `if (!values.Any()) throw new ArgumentException("Input sequence is empty.");`
    *   LINQ imports: Since I cannot add `using` statements, I must rely on fully qualified names or assume the environment has them. But the constraint says "Do NOT add using statements". This implies I should write code that compiles without explicit `using` directives in the file content provided. However, `System`, `System.Linq`, etc., are needed. In a single file context without `using` statements, I must use fully qualified names like `System.Linq.Enumerable.Average`.
    *   Wait, usually "Do NOT add using statements" means don't write `using System;` at the top. But if I don't import them, `Average` won't resolve unless I use `System.Linq.Enumerable.Average`.
    *   Actually, often in these prompts, the expectation is that the code provided is the content of a file. If I can't add using statements, I must qualify types.
    *   Median logic:
        *   Sort: `values.OrderBy(x => x).ToList()` or similar. Since I can't use `using`, I need to be careful with LINQ extensions.
        *   Actually, `Enumerable` is in `System.Linq`.
        *   Let's check the constraint again: "Do NOT add using statements." This is a hard constraint. So I must write `System.Linq.Enumerable.Average`.
    *   Return type: The prompt specifies `(double Mean, double Median, double Min, double Max)`. In C#, this is a named tuple.
    *   Named Tuple Syntax: `return (Mean, Median, Min, Max);` works for tuples. But the prompt says "Return the four values as a named tuple".
    *   Wait, does it want a `record`? "Use positional record syntax when the prompt specifies records." The return type is specified as `(double Mean, double Median, double Min, double Max)`. This is tuple syntax with names. It doesn't explicitly ask for a `record` type definition, but rather a return value structure.
    *   However, C# named tuples are often implemented via `return (Mean, Median, Min, Max);`.
    *   Let's refine the Median calculation without using `using`.
    *   Sorting: `var sorted = values.OrderBy(x => x).ToList();` -> `System.Linq.Enumerable.OrderBy`, `System.Collections.Generic.List`.
    *   Wait, `ToList()` returns a `List<T>`. I need to ensure I don't rely on implicit imports.
    *   Actually, writing fully qualified names is tedious but necessary given the constraint "Do NOT add using statements".
    *   Wait, if I write code that doesn't have `using` statements, it won't compile in a standard project unless those namespaces are already imported by the test runner or the file itself. But the instruction says "Return ONLY valid C# code in a single file." and "Do NOT add using statements". This implies the code *inside* the response should not contain `using`.
    *   To make it valid C# without `using` statements, I must use fully qualified names for all types.

3.  **Refining Logic:**

    *   Class: `public static class SurveyAnalyzer`
    *   Method: `public static (double Mean, double Median, double Min, double Max) ComputeStats(IEnumerable<double> values)`
    *   Empty check: `if (!values.Any()) throw new System.ArgumentException("Values cannot be empty.");`
    *   Mean: `var mean = values.Average();` -> `System.Linq.Enumerable.Average`.
    *   Min: `var min = values.Min();` -> `System.Linq.Enumerable.Min`.
    *   Max: `var max = values.Max();` -> `System.Linq.Enumerable.Max`.
    *   Median:
        *   Need to sort. `var sorted = values.OrderBy(x => x).ToList();`
        *   Fully qualified: `System.Linq.Enumerable.OrderBy`, `System.Collections.Generic.List`.
        *   Count: `sorted.Count`.
        *   Index logic: `int count = sorted.Count; int midIndex = count / 2; double median; if (count % 2 == 0) { median = (sorted[midIndex - 1] + sorted[midIndex]) / 2.0; } else { median = sorted[midIndex]; }`
    *   Return: `return (mean, median, min, max);`

4.  **Handling "Do NOT add using statements":**
    *   If I don't write `using System.Linq;`, then `Average` is not visible unless I use `System.Linq.Enumerable.Average`.
    *   However, `values` is `IEnumerable<double>`. This implies `System.Collections.Generic`.
    *   To be safe and strictly follow "Do NOT add using statements", I will qualify everything.

5.  **Handling "Use positional record syntax when the prompt specifies records":**
    *   The return type is `(double Mean, double Median, double Min, double Max)`. This is a named tuple. It is not explicitly a `record` type definition in the code (like `public record Stats(double Mean...)`).
    *   However, C# 7.3+ supports tuples.
    *   The prompt says "Return the four values as a named tuple".
    *   Does it want me to define a record? No, it asks for the return type syntax `(double Mean, double Median, double Min, double Max)`.
    *   Wait, looking closely at "Use positional record syntax when the prompt specifies records." This is a general instruction. The prompt does not specify a record type definition, it specifies a tuple return value. So I will use standard tuple syntax for the return value.

6.  **Addressing .NET 10 / xUnit v3:**
    *   These are testing frameworks. They don't affect the source code structure much, other than ensuring modern C# features (like tuples) are supported. Tuples were introduced in C# 7.0.
    *   The constraint "Do NOT add using statements" is tricky for `System.ArgumentException`. I must use `System.ArgumentException`.

7.  **Final Code Structure:**

Wait, `values.Any()` requires `System.Collections.Generic`. `Average` requires `System.Linq.Enumerable`. `OrderBy` requires `System.Linq.Enumerable`. `ToList` requires `System.Collections.Generic.List`.
    Since I cannot add `using`, I must qualify these.

    One detail: `values.OrderBy(x => x).ToList()`.
    `System.Linq.Enumerable.OrderBy` returns `IOrderedEnumerable<double>`.
    `System.Collections.Generic.List<T>` constructor takes `IEnumerable<T>`.
    So `System.Collections.Generic.List<System.Double>.List`? No, just `System.Collections.Generic.List<double>`.

    Wait, `values.Any()` is an extension method on `IEnumerable<T>`. It's in `System.Linq.Enumerable`.
    Actually, `Any` is defined in `System.Collections.Generic` for `ICollection`, but `IEnumerable` has it via LINQ.
    To be precise: `System.Linq.Enumerable.Any`.

    Let's check the constraint "Do NOT add using statements".
    If I write code without `using`, it won't compile unless fully qualified.
    So:
    `values.Any()` -> `System.Linq.Enumerable.Any(values)`
    `values.Average()` -> `System.Linq.Enumerable.Average(values)`
    `values.Min()` -> `System.Linq.Enumerable.Min(values)`
    `values.Max()` -> `System.Linq.Enumerable.Max(values)`
    `values.OrderBy(x => x).ToList()` -> `System.Linq.Enumerable.OrderBy(values, System.Func<double, double>)`? No, lambda is fine. But `ToList` needs to be fully qualified. `System.Collections.Generic.List<System.Double>.List`.

    Wait, `System.Func<double, double>` isn't needed for the lambda syntax if I just write `x => x`.
    The issue is `OrderBy` and `ToList`.
    `System.Linq.Enumerable.OrderBy` returns `IOrderedEnumerable<T>`.
    `System.Collections.Generic.List<T>.AddAll`? No, `ToList()` is an extension method on `IEnumerable<T>` in `System.Linq.Enumerable`.
    So: `System.Linq.Enumerable.ToList(sorted)`.

    Wait, I need to make sure the code compiles without `using` statements.
    If I use `values.Any()`, it relies on `System.Linq.Enumerable.Any`.
    If I use `values.Average()`, it relies on `System.Linq.Enumerable.Average`.
    If I use `values.Min()`, it relies on `System.Linq.Enumerable.Min`.
    If I use `values.Max()`, it relies on `System.Linq.Enumerable.Max`.
    If I use `values.OrderBy(...)`, it relies on `System.Linq.Enumerable.OrderBy`.
    If I use `.ToList()`, it relies on `System.Linq.Enumerable.ToList`.

    So the code should look like: