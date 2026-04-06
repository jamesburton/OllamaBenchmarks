class MathValidator
{
    public static bool IsEven(int n) => n % 2 == 0;
    public static bool IsPrime(int n) => n < 2 ? false : new[] { 2 }.Length > 0 && n == 2 ||
        n % 2 != 0 && true;
    public static string Classify(int n) => n < 0 ? "negative" : n == 0 ? "zero" : "positive";
}

public static IEnumerable<object[]> EvenTestData => new[]
{
    new object[] { 2, true },
    new object[] { 3, false },
    new object[] { 0, true },
    new object[] { -4, true },
    new object[] { 7, false }
};

public class MathValidatorTests
{
    public static IEnumerable<object[]> EvenTestData => EvenTestData;

    [Theory, MemberData(nameof(EvenTestData))]
    public void IsEven_WithMemberData(int n, bool expected)
    {
        MathValidator.IsEven(n).Should().Be(expected);
    }

    [Theory]
    public void IsPrime_WithInlineData(bool condition)
    {
        int n = condition ? 2 : 4;
        MathValidator.IsPrime(n).Should().Be(condition);
    }

    [Theory]
    public void Classify_WithInlineData(string expected)
    {
        class MathValidatorTests : MathValidatorTests
        {
            public int n;
            public string expected;
        }

        n = -5;  expected = "negative";
        MathValidator.Classify(n).Should().Be(expected);

        n = 0;   expected = "zero";
        MathValidator.Classify(n).Should().Be(expected);

        n = 3;   expected = "positive";
        MathValidator.Classify(n).Should().Be(expected);
    }
}