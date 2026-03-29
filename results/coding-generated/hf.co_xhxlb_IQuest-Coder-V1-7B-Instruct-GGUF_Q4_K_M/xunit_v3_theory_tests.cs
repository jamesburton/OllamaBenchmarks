using System;
using System.Collections.Generic;
using xunit;
using AwesomeAssertions;

public class StringProcessorTests
{
    private readonly StringProcessor _processor;

    public StringProcessorTests()
    {
        _processor = new StringProcessor();
    }

    [Fact]
    public void Truncate_NullInput_ReturnsEmptyString()
    {
        string result = _processor.Truncate(null, 10);
        result.Should().Be("");
    }

    [Fact]
    public void Truncate_EmptyString_ReturnsEmptyString()
    {
        string result = _processor.Truncate("", 10);
        result.Should().Be("");
    }

    [Fact]
    public void Truncate_StringShorterThanMaxLength_ReturnsOriginalString()
    {
        string input = "Hello World";
        string result = _processor.Truncate(input, 20);
        result.Should().Be(input);
    }

    [Fact]
    public void Truncate_StringEqualToMaxLength_ReturnsOriginalString()
    {
        string input = "Hello World";
        string result = _processor.Truncate(input, 11);
        result.Should().Be(input);
    }

    [Fact]
    public void Truncate_StringLongerThanMaxLength_ReturnsTruncatedString()
    {
        string input = "This is a very long string that should be truncated";
        string result = _processor.Truncate(input, 20);
        result.Should().Be("This is a very long strin...");
    }

    [Fact]
    public void Truncate_MaxLengthZero_ReturnsEmptyString()
    {
        string input = "Hello World";
        string result = _processor.Truncate(input, 0);
        result.Should().Be("");
    }

    [Fact]
    public void Truncate_NegativeMaxLength_ThrowsArgumentException()
    {
        Action action = () => _processor.Truncate("test", -1);
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CountWords_NullString_ReturnsZero()
    {
        int result = _processor.CountWords(null);
        result.Should().Be(0);
    }

    [Fact]
    public void CountWords_EmptyString_ReturnsZero()
    {
        int result = _processor.CountWords("");
        result.Should().Be(0);
    }

    [Fact]
    public void CountWords_SingleWord_ReturnsOne()
    {
        int result = _processor.CountWords("Hello");
        result.Should().Be(1);
    }

    [Fact]
    public void CountWords_MultipleWords_ReturnsCorrectCount()
    {
        int result = _processor.CountWords("Hello World Test");
        result.Should().Be(3);
    }

    [Fact]
    public void CountWords_ExtraSpaces_HandlesCorrectly()
    {
        int result = _processor.CountWords("   Multiple   Spaces   ");
        result.Should().Be(2);
    }

    [Fact]
    public void CountWords_OnlySpaces_ReturnsZero()
    {
        int result = _processor.CountWords("   ");
        result.Should().Be(0);
    }

    [Fact]
    public void IsPalindrome_NullString_ReturnsFalse()
    {
        bool result = _processor.IsPalindrome(null);
        result.Should().BeFalse();
    }

    [Fact]
    public void IsPalindrome_EmptyString_ReturnsFalse()
    {
        bool result = _processor.IsPalindrome("");
        result.Should().BeFalse();
    }

    [Fact]
    public void IsPalindrome_SingleCharacter_ReturnsTrue()
    {
        bool result = _processor.IsPalindrome("a");
        result.Should().BeTrue();
    }

    [Fact]
    public void IsPalindrome_SimplePalindrome_ReturnsTrue()
    {
        bool result = _processor.IsPalindrome("racecar");
        result.Should().BeTrue();
    }

    [Fact]
    public void IsPalindrome_NonPalindrome_ReturnsFalse()
    {
        bool result = _processor.IsPalindrome("hello");
        result.Should().BeFalse();
    }

    [Fact]
    public void IsPalindrome_MixedCase_ReturnsTrue()
    {
        bool result = _processor.IsPalindrome("RaceCar");
        result.Should().BeTrue();
    }

    [Fact]
    public void IsPalindrome_NonPalindromeWithSpaces_ReturnsFalse()
    {
        bool result = _processor.IsPalindrome("race car");
        result.Should().BeFalse();
    }

    [Fact]
    public void IsPalindrome_PalindromeWithPunctuation_ReturnsTrue()
    {
        bool result = _processor.IsPalindrome("A man, a plan, a canal: Panama");
        result.Should().BeTrue();
    }

    [Fact]
    public void IsPalindrome_PalindromeWithNumbers_ReturnsTrue()
    {
        bool result = _processor.IsPalindrome("A1B2C2B1A");
        result.Should().BeTrue();
    }
}