public static bool IsEven(int n) => n % 2 == 0;
    public static bool IsPrime(int n) => n >= 2 && !HasDivisor(n);
    private static bool HasDivisor(int n) => range(2, Math.Sqrt(n), i => n % i == 0).Any();
    public static string Classify(int n) => n < 0 ? "negative" : n == 0 ? "zero" : "positive";

    public static IEnumerable<int> range(int from, int to, Func<int, int> step)
    {
        var list = new List<int>();
        for (var i = from; i < to; i += step(i))
        {
            list.Add(i);
        }
        return list.Cast<int>();
    }

    public static void Main()
    {
        // This is only for demonstration; tests will use AwesomeAssertions
    }

    public static IEnumerable<object[]> EvenTestData => new object[]
    {
        new object[] { 2, true },
        new object[] { 3, false },
        new object[] { 0, true },
        new object[] { -4, true },
        new object[] { 7, false }
    };
}

public class MathValidatorTests
{
    public static IEnumerable<object[]> EvenTestData => EvenTestData;

    [Theory]
    [MemberData(nameof(EvenTestData))]
    public void IsEven_WithMemberData(int n, bool expected) => MathValidator.IsEven(n).Should().Be(expected);

    [Theory]
    [InlineData(2, true)]
    [InlineData(3, true)]
    [InlineData(4, false)]
    [InlineData(1, false)]
    [InlineData(17, true)]
    [InlineData(0, false)]
    public void IsPrime_WithInlineData(int n, bool expected) => MathValidator.IsPrime(n).Should().Be(expected);

    [Theory]
    [InlineData(-5, "negative")]
    [InlineData(0, "zero")]
    [InlineData(3, "positive")]
    public void Classify_WithInlineData(int n, string expected) => MathValidator.Classify(n).Should().Be(expected);
}