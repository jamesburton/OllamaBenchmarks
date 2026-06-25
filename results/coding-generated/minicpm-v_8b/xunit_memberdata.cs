using System;
using AwesomeAssertions;
using Xunit;

namespace MathValidatorTests
{
    public class MathValidatorTests
    {
        [Theory]
        [InlineData(2, true)]
        [InlineData(3, false)]
        [InlineData(0, true)]
        [InlineData(-4, true)]
        [InlineData(7, false)]
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
        public void IsPrime_WithInlineData(int n)
        {
            MathValidator.IsPrime(n);
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
}