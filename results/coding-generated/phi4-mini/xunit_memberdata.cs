using System;
using Xunit;
using AwesomeAssertions;

public static class MathValidator
{
    public static bool IsEven(int n) => n % 2 == 0;

    public static bool IsPrime(int n)
    {
        if (n < 2) return false;
        for (int i = 2; i <= Math.Sqrt(n); i++)
            if (n % i == 0) return false;
        return true;
    }

    public static string Classify(int n)
    {
        return n switch
        {
            < 0 => "negative",
            0 => "zero",
            > 0 => "positive"
        };
    }
}

public class MathValidatorTests
{
    [Theory]
    [MemberData(nameof(MathValidator.EvenTestData))]
    public void IsEven_WithMemberData(int n, bool expected)
    {
        Assert.True(n is int);
        AwesomeAssertions.Assertion.IsTrue(IsEven(n).Should().Be(expected));
    }

    [Theory]
    [InlineData(2, true)]
    [InlineData(3, false)]
    [InlineData(0, true)]
    [InlineData(-4, true)]
    [InlineData(7, false)]
    [InlineData(1, false)]
    [InlineData(17, true)]
    [InlineData(0, false)]
    public void IsPrime_WithInlineData(int n, bool expected)
    {
        Assert.True(n is int);
        AwesomeAssertions.Assertion.IsTrue(IsPrime(n).Should().Be(expected));
    }

    [Theory]
    [InlineData(-5, "negative")]
    [InlineData(0, "zero")]
    [InlineData(3, "positive")]
    public void Classify_WithInlineData(int n)
    {
        Assert.True(n is int);
        AwesomeAssertions.Assertion.IsTrue(MathValidator.Classify(n).Should().Be("expected string"));
    }

    public static IEnumerable<object[]> EvenTestData
    {
        yield return new object[] { 2, true };
        yield return new object[] { 3, false };
        yield return new object[] { 0, true };
        yield return new object[] { -4, true };
        yield return new object[] { 7, false };
        yield return new object[] { 1, false };
        yield return new object[] { 17, true };
        yield return new object[] { 0, false };
    }
}