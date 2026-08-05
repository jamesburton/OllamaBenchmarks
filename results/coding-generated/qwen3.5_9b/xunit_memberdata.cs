public static class MathValidator {
    public static bool IsEven(int n) => n % 2 == 0;

    public static bool IsPrime(int n) {
        if (n < 2) return false;
        for (int i = 2; i * i <= n; i++) {
            if (n % i == 0) return false;
        }
        return true;
    }

    public static string Classify(int n) =>
        n < 0 ? "negative" : n == 0 ? "zero" : "positive";
}

public class MathValidatorTests {
    [Theory]
    [MemberData(nameof(EvenTestData))]
    public void IsEven_WithMemberData(int n, bool expected) {
        Assert.Multiple(() => { }); // Placeholder to ensure test runs if needed, but prompt asks for specific assertions. 
        // Wait, I need to use AwesomeAssertions here as per constraints "Use AwesomeAssertions .Should()".
    }

    public static IEnumerable<object[]> EvenTestData => new[] {
        new object[] { 2, true },
        new object[] { 3, false },
        new object[] { 0, true },
        new object[] { -4, true },
        new object[] { 7, false }
    };

    [Theory]
    public void IsEven_WithMemberData(int n, bool expected) => MathValidator.IsEven(n).Should().Be(expected);

    [Fact] // Wait, prompt says "A `[Theory]` test `IsPrime_WithInlineData`". I should use Theory.
}