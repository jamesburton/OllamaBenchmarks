public static class MathValidator
{
    public static bool IsEven(int n)
    {
        return n % 2 == 0;
    }

    public static bool IsPrime(int n)
    {
        if (n < 2) return false;
        for (int i = 2; i * i <= n; i++)
        {
            if (n % i == 0) return false;
        }
        return true;
    }

    public static string Classify(int n)
    {
        if (n < 0) return "negative";
        if (n == 0) return "zero";
        return "positive";
    }
}

public class MathValidatorTests
{
    public static IEnumerable<object[]> EvenTestData =>
        new object[]
        {
            new object[] { 2, true },
            new object[] { 3, false },
            new object[] { 0, true },
            new object[] { -4, true },
            new object[] { 7, false }
        };

    [Theory, MemberData(nameof(EvenTestData))]
    public void IsEven_WithMemberData(int n, bool expected)
    {
        MathValidator.IsEven(n).Should().Be(expected);
    }

    [Theory, InlineData(2, true)]
    [Theory, InlineData(3, true)]
    [Theory, InlineData(4, false)]
    [Theory, InlineData(1, false)]
    [Theory, InlineData(17, true)]
    [Theory, InlineData(0, false)]
    public void IsPrime_WithInlineData(int n, bool expected)
    {
        MathValidator.IsPrime(n).Should().Be(expected);
    }

    [Theory, InlineData(-5, "negative")]
    [Theory, InlineData(0, "zero")]
    [Theory, InlineData(3, "positive")]
    public void Classify_WithInlineData(int n, string expected)
    {
        MathValidator.Classify(n).Should().Be(expected);
    }
}