public static class MathValidator
{
    public static bool IsEven(int n) => n % 2 == 0;
    public static bool IsPrime(int n) => n >= 2 && Enumerable.Range(2, (int)Math.Sqrt(n) - 1).All(i => n % i != 0);
    public static string Classify(int n) => n < 0 ? "negative" : n == 0 ? "zero" : "positive";
}

public class MathValidatorTests
{
    public static IEnumerable<object[]> EvenTestData => new[]
    {
        new object[] { 2, true },
        new object[] { 3, false },
        new object[] { 0, true },
        new object[] { -4, true },
        new object[] { 7, false }
    };

    [Theory]
    [MemberData(nameof(EvenTestData))]
    public void IsEven_WithMemberData(int n, bool expected)
    {
        MathValidator.IsEven(n).Should().Be(expected);
    }

    [Theory]
    [InlineData(2, true), InlineData(3, true), InlineData(4, false), InlineData(1, false), InlineData(17, true), InlineData(0, false)]
    public void IsPrime_WithInlineData(int n, bool expected)
    {
        MathValidator.IsPrime(n).Should().Be(expected);
    }

    [Theory]
    [InlineData(-5, "negative"), InlineData(0, "zero"), InlineData(3, "positive")]
    public void Classify_WithInlineData(int n, string expected)
    {
        MathValidator.Classify(n).Should().Be(expected);
    }
}