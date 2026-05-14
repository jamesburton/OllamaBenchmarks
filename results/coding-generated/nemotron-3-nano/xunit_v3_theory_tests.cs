using AwesomeAssertions;
using Xunit;

public class StringProcessor
{
    public string Truncate(string input, int maxLength)
    {
        if (input is null) return "";
        return input.Length <= maxLength ? input : input[..maxLength] + "...";
    }

    public int CountWords(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return 0;
        return input.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
    }

    public bool IsPalindrome(string input)
    {
        if (string.IsNullOrEmpty(input)) return false;
        var cleaned = input.ToLowerInvariant().Replace(" ", "");
        return cleaned.SequenceEqual(cleaned.Reverse());
    }
}

public class Tests
{
    [Theory]
    [InlineData("", 0)]
    [InlineData("a", 1)]
    [InlineData("hello world", 2)]
    [InlineData("  word  ", 1)]
    [InlineData(null, 0)]
    public void CountWords_Should_Return_Correct_Count(string input, int expected)
    {
        var result = new StringProcessor().CountWords(input);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, 0)]
    [InlineData("", 0)]
    [InlineData("abc", 3)]
    [InlineData("abcd", 4)]
    [InlineData("abcdef", 6)]
    public void Truncate_Should_Handle_Different_String_Lengths(string input, int maxLength)
    {
        var result = new StringProcessor().Truncate(input, maxLength);
        if (input is null) result.Should().Be("");
        else if (input.Length <= maxLength) result.Should().Be(input);
        else result.Should().Be(input.Substring(0, maxLength) + "...");
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("a", true)]
    [InlineData("aba", true)]
    [InlineData("abba", true)]
    [InlineData("ABCD", false)]
    [InlineData("A b a", true)]
    public void IsPalindrome_Should_Handle_Palindrome_Cases(string input, bool expected)
    {
        var result = new StringProcessor().IsPalindrome(input);
        result.Should().Be(expected);
    }
}