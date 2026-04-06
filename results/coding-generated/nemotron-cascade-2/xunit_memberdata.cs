public static class MathValidator
{
    public static bool IsEven(int n) => n % 2 == 0;

    public static bool IsPrime(int n)
    {
        if (n < 2) return false;
        if (n == 2) return true;
        if (n % 2 == 0) return false;
        for (int i = 3; i * i <= n; i += 2)
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
    public static IEnumerable<object[]> EvenTestData
    {
        get
        {
            yield return new object[] { 2, true };
            yield return new object[] { 3, false };
            yield return new object[] { 0, true };
            yield return new object[] { -4, true };
            yield return new object[] { 7, false };
        }
    }

    [Theory]
    [MemberData(nameof(EvenTestData))]
    public void IsEven_WithMemberData(int n, bool expected)
    {
        MathValidator.IsEven(n).Should().Be(expected);
    }

    [Theory]
    [InlineData(2, true)]
    [InlineData(3, true)]
    [InlineData(4, false)]
    [InlineData(1, false)]
    [InlineData(17, true)]
    [InlineData(0, false)]
    public void IsPrime_WithInlineData(int n, bool expected)
    {
        MathValidator.IsPrime(n).Should().Be(expected);
    }

    [Theory]
    [InlineData(-5, "negative")]
    [InlineData(0, "zero")]
    [InlineData(3, "positive")]
    public void Classify_WithInlineData(int n, string expected)
    {
        MathValidator.Classify(n).Should().Be(expected);
    }
}