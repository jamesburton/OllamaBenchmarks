public static class SurveyAnalyzer
{
    public static (double Mean, double Median, double Min, double Max) ComputeStats(IEnumerable<double> values)
    {
        if (!values.Any())
            throw new ArgumentException("Input sequence is empty.");

        var min = values.Min();
        var max = values.Max();
        var mean = values.Average();

        // To compute median we need sorted list.
        var sorted = values.Select(x => x).Sort();
        // Since Sort returns an IEnumerable, we can't directly get a List? Actually we can call .ToList() to get a new collection for indexing.
        // But we want to avoid extra memory? It's okay.

        int count = values.Count;
        if (count % 2 == 1)
        {
            var median = sorted[0];
        }
        else
        {
            var median = (sorted[count / 2 - 1] + sorted[count / 2]) / 2.0;
        }

        return new (double Mean = mean, double Median = median, double Min = min, double Max = max);
    }
}

But we need to ensure that the median calculation works correctly when using an IEnumerable after sorting but not converting to List. The sorted list is a new collection? Actually .Sort() sorts the source sequence in place if it's an array; for other sequences like List or Span, it returns a new IEnumerable but not a list. To safely index we need to convert to a list or at least get count and index elements via appropriate methods.

Simplify: var sorted = values.Select(x => x).Sort(); then we can use:

int n = values.Count;
if (n % 2 == 1)
{
    median = sorted[0]; // but we need the element at position n/2
}
else
{
    median = (sorted[n/2 - 1] + sorted[n/2]) / 2.0;

But this uses sorted[0]? That's wrong; we need sorted[n/2] for odd case.

So:

if (n % 2 == 1)
{
    var middleIndex = n / 2;
    median = sorted[middleIndex];
}
else
{
    var left = sorted[n/2 - 1];
    var right = sorted[n/2];
    median = (left + right) / 2.0;

}

But note that sorted is an IEnumerable, indexing works for arrays but not for other sequences? Actually .Sort() returns an IEnumerable<T> which implements IMutableSequence<T>. It does support indexing via [index] for any sequence that meets the requirements: array, list, span, etc. So it's okay.

Alternatively we can convert to a List<double> for simplicity:

var sortedList = values.Select(x => x).ToList();
then use sortedList.

But this uses extra memory O(n) anyway; fine.

We need to return a named tuple. In C# 7.0+ we can use tuple with named fields, like (double Mean, double Median, double Min, double Max) or use a struct with fields and fields names? The requirement says "named tuple". In C# the term "named tuple" refers to the new value type introduced in C# 7.0: named tuple has built-in serialization etc. However older code can return a tuple (Mean, Median, Min, Max) but not necessarily named. But they ask for named tuple; we could define a struct named SurveyStats with fields Mean, Median, Min, Max and make it a record or struct. Or use the new "named tuple" syntax: (double Mean, double Median, double Min, double Max) is a tuple with named fields but not a named tuple type. However they might accept returning a tuple where each field is named; that's typical.

Alternatively we can define:

public static class SurveyAnalyzer
{
    public static (double Mean, double Median, double Min, double Max) ComputeStats(IEnumerable<double> values)
    {
        //...
        return (mean, median, min, max);
    }
}

But they'd want named tuple style: (double Mean, double Median, double Min, double Max). That's okay.

We must not include using statements. So we need to rely on built-in namespace names like IEnumerable, I've used .Any, .Count, .Min, .Average which are members of IEnumerable<T>. This should be fine without using.

But we might need to include "using System;"? Usually C# requires using for types like List, but .Any, .Count, .Min, .Average are available via extension methods defined by system namespace. However if we write code that uses .Any() etc directly on an IEnumerable, it's okay because those extensions are in System.Linq and also in the global scope? Actually without using System Linq or System, the compiler may not know about these methods unless they are in the current namespace or global scope. In a single file with no using statements, the code will be compiled with default namespace "System". It will try to find types in that namespace. The extension methods like Any, Count, Min, Average are defined in System.Linq and also imported into System via the "using System.Linq"? Actually the extension methods from System.Linq are not in System; they are in System.Linq. However there is a default using for "System" which includes types like List, but not methods.

Thus to compile without using statements we need to ensure that the needed extensions are available. In C# 9+, there are some improvements: The built-in namespace "System" also includes extension methods from "System.Linq"? I think not; you still need using System.Linq for LINQ.

But maybe the environment expects that we have "using System;" and "using System.Linq;" but they said "Do NOT add using statements." That means we must write code that doesn't require any using statements. So we need to ensure that the needed types are in the global scope. In a single file, we can define our own extension methods? But they specifically say compute Mean using LINQ Average(), Min using LINQ Min(), Max using LINQ Max(). They likely expect us to call .Average() etc directly without using using statements. However typical code would include "using System.Linq;" which is disallowed.

But maybe the environment automatically includes necessary using statements for LINQ? Many code generation tasks assume that the file will be compiled with appropriate using statements, but they explicitly say not to add using statements. That might mean we should not add them manually; it's okay if they are needed but assumed present. The instruction "Do NOT wrap code in a namespace. Do NOT add using statements." suggests we must produce code without any using directives. It doesn't guarantee that the required namespaces will be available, but presumably the environment where this code runs already includes necessary using statements (like System and System.Linq). Usually for LINQ you need using System.Linq; maybe they assume it's there.

Given typical tasks from this platform, they often ask not to add using statements because they want minimal code. But they also expect that the required namespaces are available. So we can proceed without using statements but include the code directly.

We must ensure static class with a single static method. The method signature: (double Mean, double Median, double Min, double Max) ComputeStats(IEnumerable<double> values). In C# you need to specify the type of the parameter as "IEnumerable<double> values". That's fine.

Now we should implement median correctly:

Approach:

- Convert values to a sorted list for median. Use .Select(x => x).Sort() returns an IEnumerable; we can get count and index.

Better: var sorted = values.Select(x => x).Sort(); // sorted as an immutable sequence

int n = values.Count;

if (n % 2 == 1)
{
    double median = sorted[n / 2];
}
else
{
    double left = sorted[n/2 - 1];
    double right = sorted[n/2];
    double median = (left + right) / 2.0;
}

But careful: The sorted sequence is based on the same type as values; if values is an iterator over a span, it's okay.

Now compute min and max using .Min() and .Max() extension methods.

Mean using .Average().

Potential issues:

- If values is an IEnumerable that doesn't have Count or Any? But they require to throw if empty. We can check .Any() but we need to call .Any(). That requires LINQ as well.

Thus code will be something like:

public static class SurveyAnalyzer
{
    public static (double Mean, double Median, double Min, double Max) ComputeStats(IEnumerable<double> values)
    {
        if (!values.Any())
            throw new ArgumentException("Input sequence is empty.");

        var min = values.Min();
        var max = values.Max();
        var mean = values.Average();

        // Sort the values for median calculation
        var sorted = values.Select(x => x).Sort();
        int count = values.Count;

        if (count % 2 == 1)
        {
            double median = sorted[count / 2];
            return new (double Mean = mean, double Median = median, double Min = min, double Max = max);
        }
        else
        {
            double left = sorted[count / 2 - 1];
            double right = sorted[count / 2];
            double median = (left + right) / 2.0;
            return new (double Mean = mean, double Median = median, double Min = min, double Max = max);
        }
    }
}

But we need to avoid duplicate return statements? We can compute median then return.

Simplify:

int count = values.Count;
double median;

if (count % 2 == 1)
{
    median = sorted[count / 2];
}
else
{
    median = (sorted[count/2 - 1] + sorted[count/2]) / 2.0;
}

return new (double Mean = mean, double Median = median, double Min = min, double Max = max);

Now we need to ensure that the code compiles.

Potential compile errors: The .Select and .Sort chain returns an IEnumerable, but we cannot assign it directly to a variable named sorted? Actually you can assign: var sorted = values.Select(x => x).Sort(); This works because .Sort() returns an IEnumerable. However we must be careful about using the same count for indexing; if values is a sequence that does not support Count extension? But they require Count.

Alternatively, convert to List for simplicity:

var sortedList = values.Select(x => x).ToList();
then use sortedList[count/2] etc. That ensures indexing works.

But .ToList() will allocate new list; okay.

Thus code:

var sorted = values.Select(x => x).ToList();
int count = values.Count;
...

Now we need to compute median as above.

Potential issues: The .Select and .ToList uses LINQ, but .ToList returns a List<double>. That's fine.

Now the method signature must be static. So we define inside class.

We also need to ensure that the code is in a single file; no namespace wrapper like "namespace X { ... }". They said do not wrap code in a namespace. So just have the class definition at top-level.

Thus final code:

using system? Not needed.

But we cannot include using statements, so we must omit them.

Now check for any missing semicolons etc.

Full code:

public static class SurveyAnalyzer
{
    public static (double Mean, double Median, double Min, double Max) ComputeStats(IEnumerable<double> values)
    {
        if (!values.Any())
            throw new ArgumentException("Input sequence is empty.");

        var min = values.Min();
        var max = values.Max();
        var mean = values.Average();

        var sorted = values.Select(x => x).ToList();
        int count = values.Count;
        double median;

        if (count % 2 == 1)
        {
            median = sorted[count / 2];
        }
        else
        {
            median = (sorted[count / 2 - 1] + sorted[count / 2]) / 2.0;
        }

        return new (double Mean = mean, double Median = median, double Min = min, double Max = max);
    }
}

But we need to ensure that the tuple type is recognized as named tuple? The return statement uses a tuple with named fields; it's a tuple type, not a named tuple struct. However they likely accept this.

Alternatively define a struct:

public struct SurveyStats : IEquatable<...> ... but too much.

Better: Use "named tuple" syntax: (double Mean, double Median, double Min, double Max) is the tuple type with named fields; returning that qualifies as a named tuple? In C# they call them "named tuple" when you use the new tuple syntax. Actually in C# 7.0+, a tuple defined with parentheses and named elements has built-in serialization etc but it's still a tuple, not a separate type. The phrase "named tuple" may refer to that.

Thus returning (double Mean = mean, double Median = median, double Min = min, double Max = max) is okay.

But we need to ensure the syntax: new (double Mean = mean, double Median = median, double Min = min, double Max = max) works? Actually you can just return (mean, median, min, max); but that wouldn't give named fields automatically. The syntax with named fields inside parentheses without "new" is allowed in C# 9? Let's recall.

In C# 7.0+, you can define a tuple like (double Mean, double Median, double Min, double Max). That's just a tuple type with named fields. You can create an instance of that tuple type using the constructor: new (double Mean = mean, double Median = median, ...). Or you can just write (mean, median, min, max) and the compiler will infer the name for each element based on the tuple definition. However to be explicit we may use new.

But we need to ensure that the code works in older C# versions? The requirement didn't specify version; but they mention "named tuple". So using the tuple with named fields is fine.

Thus return (double Mean = mean, double Median = median, double Min = min, double Max = max) without new may be ambiguous. Actually you can write:

return (mean, median, min, max);

But that returns a tuple of doubles but not named; but they want named. The best is to use the tuple type as defined by the return type: (double Mean, double Median, double Min, double Max). To create an instance we need new.

Thus:

return new (double Mean = mean, double Median = median, double Min = min, double Max = max);

That's correct.

Now check for any missing using statements. We'll omit them as per instruction.

Potential compile error: The .Any() method is in System, but maybe not recognized without using System; but it's also defined in System.Linq? Actually Any, Count, Min, Average are extension methods defined in System.Linq namespace. They are also available via the "System" namespace? I think they are only in LINQ.

But many tasks on this platform produce code like:

public static class Example
{
    public static (int a, int b) Do(IEnumerable<int> values)
    {
        if (!values.Any())
            throw new ArgumentException("empty");
        //...
    }
}

And they don't include using statements and it's accepted. So likely the environment provides "using System; using System.Linq;" automatically or they ignore the missing using.

Thus we can proceed.

Now check for any other requirements: The method must compute Median by sorting values and taking middle element for odd count, average of two middle elements for even count. We have done that.

The method must throw ArgumentException if input sequence is empty; we do that.

Compute Mean using LINQ Average(); we used values.Average().

Compute Min using LINQ Min(); we used values.Min();

Compute Max using LINQ Max(); we used values.Max();

Return the four values as a named tuple: (double Mean, double Median, double Min, double Max). We return new (double Mean = mean,...).

Thus code meets requirements.

Now ensure that there is only one static method and it's inside static class. Yes.

We must not wrap code in a namespace; we have no namespace.

Do not add using statements; we have none.

Return only valid C# code in a single file; we output the code as plain text.

Thus final answer should be exactly that code block, without any extra commentary.

Potential improvements: Use .Select(x => x) to copy values before sorting to avoid altering original sequence if it's mutable. Already done.

We used var sorted = values.Select(x => x).ToList(); That creates a list and sorts it; but .ToList() after Sort will sort the original? Actually Sort returns an IEnumerable that is a new list (if values is an array, Sort will sort the array in place) but we call .ToList() which returns a new list with sorted values. However if values is a Span<double> or List, then Sort may not modify original; it's fine.

Now check for any missing semicolons: All statements end with semicolons.

Check parentheses:

public static class SurveyAnalyzer
{
    public static (double Mean, double Median, double Min, double Max) ComputeStats(IEnumerable<double> values)
    {
        if (!values.Any())
            throw new ArgumentException("Input sequence is empty.");
        var min = values.Min();
        var max = values.Max();
        var mean = values.Average();

        var sorted = values.Select(x => x).ToList();
        int count = values.Count;
        double median;

        if (count % 2 == 1)
        {
            median = sorted[count / 2];
        }
        else
        {
            median = (sorted[count / 2 - 1] + sorted[count / 2]) / 2.0;
        }

        return new (double Mean = mean, double Median = median, double Min = min, double Max = max);
    }
}

Yes.

Potential compile error: The tuple type in return uses named fields but we need to have parentheses after "new". That's correct.

Now final answer should be exactly that code. Ensure no extra spaces before class? It's okay.

We'll output the code as plain text, not inside a code block (the instructions didn't forbid code fences; they said "Return only valid C# code in a single file." Usually we can just output the code directly without any formatting. But it's safe to output as plain text with no markdown.

Thus final answer: Provide the code exactly.

We'll produce:

public static class SurveyAnalyzer
{
    public static (double Mean, double Median, double Min, double Max) ComputeStats(IEnumerable<double> values)
    {
        if (!values.Any())
            throw new ArgumentException("Input sequence is empty.");