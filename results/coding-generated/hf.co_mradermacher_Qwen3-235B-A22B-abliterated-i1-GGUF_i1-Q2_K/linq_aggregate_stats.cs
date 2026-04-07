public static class SurveyAnalyzer
{
    public static (double Mean, double Median, double Min, double Max) ComputeStats(IEnumerable<double> values)
    {
        var list = values.ToList();
        if (!list.Any())
        {
            throw new ArgumentException("Input sequence is empty");
        }

        double mean = list.Average();
        double min = list.Min();
        double max = list.Max();

        var sorted = list.OrderBy(x => x).ToList();
        int count = sorted.Count;
        double median;

        if (count % 2 == 0)
        {
            median = (sorted[count / 2 - 1] + sorted[count / 2]) / 2;
        }
        else
        {
            median = sorted[count / 2];
        }

        return (mean, median, min, max);
    }
}

Wait, but wait: the problem says to throw ArgumentException if the input sequence is empty. So the check should be if list.Count == 0. Because if we call ToList(), then list.Any() would check if there are any elements. But list.Any() is equivalent to Count > 0. So that's okay.

Wait, but if the input is an empty collection, then list will be empty. So the check here is correct.

But wait, in the problem statement: the method should throw if the input sequence is empty. So regardless of how the input is passed (like, as a null reference, but no, the method expects an IEnumerable), but if the sequence has no elements, then throw.

So the check is correct.

Wait, but if the input is null, then when calling values.ToList(), it will throw a NullReferenceException. But according to the problem statement, the method should throw ArgumentException if the input sequence is empty. But perhaps the problem assumes that the input is a valid IEnumerable. But in reality, the method should check if values is null, but according to the problem statement, we don't need to handle that. The problem says to throw ArgumentException if the input sequence is empty, not if it's null. So we can proceed as before.

So the code above seems correct.

Wait, but in C#, the ToList() method will throw an ArgumentNullException if the input is null. But the problem says that the method should throw ArgumentException if the sequence is empty. So if the input is null, the method would throw ANRE, but according to the problem, we should throw ArgumentException. So perhaps we need to handle that.

Wait, but the problem says that the method should throw ArgumentException if the input sequence is empty. It doesn't say about null. So perhaps, the problem expects that passing null as the input would result in an empty sequence, but no. But in reality, the method's parameter is IEnumerable<double> values, which can be null. So in that case, when you call values.ToList(), it would throw. So to handle that, but according to the problem statement, we are not required to throw ArgumentException for null, only for empty sequences.

But in the problem statement, the method should throw ArgumentException if the input sequence is empty. So perhaps, the problem expects that the method handles the empty case, but not the null case. However, the problem says "input sequence is empty" which could be interpreted as the input being a non-null collection with zero elements, or a null collection. But in C#, the method would throw NullReferenceException when values is null when calling ToList().

But according to the problem's instruction, we need to throw ArgumentException when the sequence is empty. So perhaps, the problem expects that the input is valid, and the code should handle only the empty case.

But in the code above, if the input is null, then when we call values.ToList(), it will throw ArgumentNullException. Which would not be caught here, but according to the problem, we should throw ArgumentException. So perhaps, we need to check if the values is null, and if so, throw ArgumentException? But the problem statement says "empty sequence" which may not be the same as null. However, the problem says "input sequence", which may imply that the input is a valid IEnumerable, but not null. Or perhaps not.

But according to the problem's instructions, the method should throw ArgumentException if the input sequence is empty. So perhaps, the code should check for null, but the problem doesn't mention that. But in the problem's code, the parameter is IEnumerable<double> values, which can be null. So in that case, if the user passes null, then the code should throw ArgumentException. But according to the problem's instruction, the method should throw ArgumentException only when the sequence is empty.

Wait, but if the user passes null, then the sequence is considered empty? Or not. Maybe the problem says that the input sequence is empty, regardless of whether it's null or not. But in C#, an empty sequence is a non-null IEnumerable with zero elements. Null is a different case.

But according to the problem's instruction, the method should throw ArgumentException if the input sequence is empty. So perhaps, the problem expects that if the input is null, then it's considered an empty sequence. But that's not the case in C#. So perhaps, the problem expects that the input is a valid IEnumerable, not null, but empty.

But according to the problem's code, the method's parameter is IEnumerable<double> values, and the code should handle that.

But in the problem statement, the user says: throw ArgumentException if the input sequence is empty. So, if the input is null, then we can treat it as empty? Or not. But according to the problem's instruction, the problem says "empty sequence", which would imply that the input is a sequence with no elements, not null. So the code as written would throw ArgumentNullException when values is null, but according to the problem, the code should throw ArgumentException. So perhaps, we need to check if (values == null) or list.Count == 0. But the problem says "empty sequence", which would be list.Count ==0.

Wait, but if values is null, then values.ToList() would throw ArgumentNullException. So in order to handle the case where the input is null as an empty sequence, we need to check if values is null, and then throw ArgumentException. But according to the problem statement, the exception should be thrown "if the input sequence is empty". So perhaps, the input sequence's null-ness is considered as empty? Or not.

But the problem's example code says:

IEnumerable<double> values

So, in the problem, the user must pass a non-null collection, but empty. But in code, the code will throw if values is null. So according to the problem's instruction, we should throw ArgumentException only when the input sequence has zero elements, not when it's null. But the problem's code would throw when values is null. But according to the problem's instruction, the code should throw ArgumentException if the input sequence is empty. So perhaps, the code should check if list.Count == 0, regardless of whether values is null.

Wait, but if values is null, then list = values.ToList() would throw ArgumentNullException. So in order to pass the test case, the problem says that the input sequence is empty. So perhaps, the code should check if the list is empty. Which is what we have.

In that case, the code as written will throw ArgumentException if the list is empty, regardless of whether values is null. But if values is null, then ToList() would throw. But according to the problem's instruction, the exception should be ArgumentException, but in that case, the code would throw ArgumentNullException. Which would not meet the problem's requirements.

But according to the problem's instruction, the user is to throw ArgumentException if the input sequence is empty. So, in the problem, if the input is null, then the problem's code would throw ArgumentNullException, which is not desired.

But according to the problem's instruction, the method should throw ArgumentException if the input sequence is empty. So perhaps, the code should check if values is null or empty.

But how?

Wait, in C#, if the method is called with null as the argument, then values == null. So:

IEnumerable<double> values = null;

Then, list = values.ToList() would throw ArgumentNullException.

But according to the problem's instruction, the method should throw ArgumentException if the input sequence is empty.

So, in order to treat null as an empty sequence, we need to check if values == null or if it's empty.

But according to the problem's instruction, the empty sequence is the only case to throw. So perhaps, the problem expects that the input is a valid IEnumerable, not null, but empty.

But the problem says "empty sequence" which could be either. So perhaps, the problem's code should check if list.Count == 0, regardless of how it's passed.

But in the code, the ToList() call would capture all elements. If values is null, then that throws. But according to the problem's instruction, we should throw ArgumentException if the input sequence is empty. So perhaps, the code should check for list.Count == 0.

But in the code written, the check is:

var list = values.ToList();
if (!list.Any())
{
    throw new ArgumentException(...);
}

Which would catch all cases where the list is empty, regardless of whether the input was null or a non-empty collection. Wait, no. If the input is null, then values.ToList() would throw ArgumentNullException. So the code as written would not catch that.

So, in order to handle null as an empty sequence, we need to check if values is null first.

But according to the problem's instruction, the method should throw ArgumentException if the input sequence is empty. So:

If the input is null, is that considered an empty sequence? Or not?

The problem says "empty sequence", which in C# terms would be a non-null IEnumerable with zero elements. So passing null would be a different error, not covered by the ArgumentException in the problem.

But according to the problem's code, the user may pass null, which would cause a NullReferenceException when calling values.ToList(). But in C#, the ToList() method checks for null, and throws ArgumentNullException.

So, in order to meet the problem's requirements, the code should throw ArgumentException only when the sequence is empty, and not when it's null. But in that case, the code would throw ArgumentNullException when values is null, which is not desired.

But the problem's instruction says "throw ArgumentException (with any message) if the input sequence is empty". So perhaps, the problem expects that the code checks if the sequence is empty, regardless of whether it's null. But in C#, the parameter would have to be checked first.

But according to the problem's instructions, the code should be written as a static class and method, and the ComputeStats method's parameter is IEnumerable<double> values.

So, in order to handle null as an empty sequence, but according to the problem's instruction, we are not told to handle null. So perhaps, the problem expects that the input is a valid IEnumerable, and that passing null would throw an exception, but not the ArgumentException. But according to the problem's instruction, the code should throw ArgumentException only when the sequence is empty.

So, the code written as before, which checks list.Any() would throw ArgumentException for empty lists, and if the input is null, then the ToList() throws ArgumentNullException. But according to the problem, the ArgumentException is required only when the sequence is empty, not when it's null.

But according to the problem's instruction, the code should throw ArgumentException when the input sequence is empty. So the code as written would handle that.

But if the input is null, then the code throws ArgumentNullException, which is not ArgumentException. But according to the problem's instruction, the code should throw ArgumentException in the empty case, but not necessarily handle null.

But the problem says "empty sequence", which implies that the input is a sequence (IEnumerable) with zero elements, not null.

So, the code written above is correct.

Now, code steps:

Check if list.Any() == false.

Throw ArgumentException.

Compute mean, min, max.

Sort list.

Compute median.

Now, in C#, tuples are named, but in the method signature, the tuple must have the elements named as (Mean, Median, Min, Max).

In C# 7.0 and above, named tuples are supported.

So the method returns (double Mean, double Median, double Min, double Max) as a tuple.

Testing code:

But in the code generated, the method returns (mean, median, min, max), which are assigned to the tuple's elements.

Now, possible mistakes:

Wait, when computing median, after sorting, the code uses list.OrderBy(x => x).ToList(). But list could be empty, but we already checked that. So okay.

But in the code:

sortedList is list.OrderBy(x => x).ToList(). Then count is taken.

But yes.

Wait, but in the code:

var list = values.ToList();
if (!list.Any())
{
    throw new ArgumentException(...);
}

So list is a list of all elements in values. If the list is empty, throw.

Then compute mean, min, max.

Then sort list and compute median.

Yes.

But in code:

list.OrderBy(x => x).ToList() creates a new list sorted.

But list.Count could be even or odd.

Yes.

Testing:

For example:

Input: [1, 2, 3, 4]

sorted list [1,2,3,4], count 4.

median = (2 + 3)/2 = 2.5.

Input: [1,3,2], sorted [1,2,3], median 2.

Yes.

Now, code:

Wait, but in C#, the ToList() after OrderBy() will create a new list sorted.

Yes.

So the code should work.

Now, check for possible errors.

Wait, if the list contains NaN values? But the problem says nothing about that. Since the method uses LINQ's Average, Min, etc., which would return NaN if the input has NaNs, but the problem doesn't mention handling that.

But according to the problem's instructions, we don't need to handle that.

Now, code.

Wait, but the problem says "return the four values as a named tuple: (double Mean, double Median, double Min, double Max)".

So the tuple elements must be named as such.

In C#, the return statement would be:

return (mean, median, min, max); but the tuple's elements are named as per the declaration.

But in the method signature, the tuple is declared with names:

public static (double Mean, double Median, double Min, double Max) ComputeStats(...)

So the return statement must assign the four values to the named tuple.

In C# 7.0+, the return statement can be:

return (mean, median, min, max); and the tuple elements will be named as per the method signature.

Wait, but wait:

No. The names in the tuple must be specified in the method signature. So the method signature must have the names, and the return statement's tuple can be assigned in order.

For example:

public static (double Mean, double Median, double Min, double Max) ComputeStats(...)

Then, when returning:

return (mean, median, min, max); 

The order matches the names. So the first item is Mean, second Median, etc.

Yes.

So the code is correct.

Now, code:

Check if all steps are covered.

- Static class SurveyAnalyzer.

- Static method ComputeStats.

- Throws ArgumentException if empty.

- Mean via Average().

- Min via Min().

- Max via Max().

- Median via sorted list.

Yes.

Now, code:

The code should not have any using statements. Since the code uses LINQ, it needs to have using System.Linq; but the problem says not to add using statements. Wait, but the problem says "Do NOT wrap code in a namespace. Do NOT add using statements."

Wait, but the code uses:

list.OrderBy(x => x).ToList();

Which requires System.Linq to be present, but the problem says not to add using statements.