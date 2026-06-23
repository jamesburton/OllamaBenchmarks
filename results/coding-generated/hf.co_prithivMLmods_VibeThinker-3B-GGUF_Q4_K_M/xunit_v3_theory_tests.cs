[Theory]
[InlineData(null, 5], ExpectedResult = "" ]
[Theory]
[InlineData("", 5], ExpectedResult = "" ]
...

public static class StringProcessor
{
    public string Truncate(string input, int maxLength)
    {
        // implementation
    }

    // other methods...
}

public class StringProcessor
{
    // implementation as given.
}

public class StringProcessorTests
{
    [Theory]
    [InlineData(null, 5]]
    public void Truncate NullInput(int maxLength, string expected)
    {
        var result = StringProcessor.Truncate(null, maxLength);
        result Should().Be(string.Empty);
    }

    // other tests...
}

[Theory]
[InlineData(null, 5]]
[ theory? Actually we need to separate each test method. Use [Theory] on a method and multiple InlineData entries.
public void Truncate_Variants()
{
    var result = StringProcessor.Truncate(input, maxLength);
    result Should().Be(...);
}

[Theory]
[InlineData(null, 5], Expected = "" ]
[ theory? Actually the attribute expects a list of pairs: `[InlineData(input, maxLength)]` and then we need to set expected result maybe via a static property or via a function that returns expected. But if we want to embed expectation inside method body using `result Should().Be(expected)`, we cannot pass expected as part of InlineData; we must compute expected based on test scenario.

We can define a separate method for each case, simpler.

Given the requirement is not strict about number of tests, but just that they are parameterized. We could have one method per case with `[Theory]` and `[InlineData]` maybe using a single argument (like max length) and then inside we compute expected via a static helper? But easier: Use `[Theory]` on each test method with multiple InlineData entries for the input and maxLength values, and inside each test body we assert accordingly.

Thus:


But we need to differentiate expected for each case. Since the expected is same for each test (empty string). So we can reuse the same assertion.

Similarly:


But note: The method returns a string; for empty input, code returns ""; so it's fine.

For string shorter than max:


For equal length: input length equals maxLength, e.g., "abc", max=3.


For longer than max: input "abcdef", max=3:


Note: The ellipsis is "...". So expected output should be "abc...".

Make sure to use `result Should().Be("abc...")` not `.Be`. Actually we need to call `.Be(expected)`. The syntax: `result Should().Be(expected)`? Let's check the documentation:

Under "Basic Assertions", they show:


So it's `Be` method. So we need to call `result Should().Be("abc...")`. Actually you need a dot: `result Should().Be("abc...")`.

Thus the correct usage is:

`result Should().Be("abc...")`.

Make sure to use correct naming.

Now for CountWords tests:

Method signature: int CountWords(string input). So we can test null, empty string, single word, multiple words, extra spaces.

We'll write separate test methods with `[Theory]` and `[InlineData]`. For each case, we pass input and expect output integer.

Test cases:

- Null input => 0.
- Empty string => 0.
- Single word "hello" => length = 5. So expected result 5.
- Multiple words: "hello world" => tokens ["hello","world"]; sum lengths = 10. Expect 10.
- Extra spaces: "   hello   world   " => same as above (10). We can test that.

Thus:


But we need to pass input only. Actually `[InlineData null]` is used for null string inputs; but the attribute expects both arguments? The method has one argument: string input. So `[InlineData(null)]` works.

For empty string:


But note the constraint says "Use [InlineData(null)] to test null string inputs (not [InlineData("")] for null." That means we must use `[InlineData(null)]` for null input, and using `" "` for empty string is fine. So above is correct.

For single word:


But we need to ensure that the method returns int; we assert with `result Should().Be(5);` Actually for int, it's `.Be`. The documentation shows `result Should().Be(42)`. So use `result Should().Be(5)`? Wait they wrote "result Should().Be(42);" but in code snippet they used `Be`, not `Is`. Let’s check the provided snippet:


But in the code block they wrote `Be`, `NotBe`. Actually there is a mix: In the snippet:


But later they wrote "Basic Assertions" with code block showing `resultShould().Be(42);`? Let's scroll.

The snippet:


result SHOULD().BE(42);
result SHOULD().NOTBE(0);
str SHOULD().BE("hello").AND.HAVELENGTH(5);
obj SHOULD().ISNULL();
obj SHOULD().NOTNULL();

Thus they use `Be`, `NotBe`, `IsNull` etc. However the "Type Assertions" snippet shows:


But we need to be careful: The method names are `Be`, `NotBe`, `IsNull`? Actually there is a separate "Type Assertions" block showing:


Thus the correct usage for int values is `.Be`. For string comparisons, we use `.Be("...")`.

Thus for CountWords returning int, we should use `.Be(expected)`. So `result Should().Be(5)`? Wait there's confusion: The snippet uses both `Be` and `Is`. Let's examine the entire snippet:


resultShould().Be(42);
resultShould().NotBe(0);
strShould().Be("hello").And.HaveLength(5);
objShould().IsNull();
objShould().NotNull();

objShould().BeOfType<Order>();
objShould().NotNull();