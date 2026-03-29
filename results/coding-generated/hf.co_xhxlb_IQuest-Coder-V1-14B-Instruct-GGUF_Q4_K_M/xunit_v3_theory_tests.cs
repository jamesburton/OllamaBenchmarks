using Xunit;
using AwesomeAssertions;

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

public class StringProcessorTests
{
    private readonly StringProcessor _processor = new StringProcessor();

    [Theory]
    [InlineData(null, 10, "")]
    [InlineData("", 10, "")]
    [InlineData("hello", 10, "hello")]
    [InlineData("hello", 5, "hello")]
    [InlineData("hello world", 5, "hello...")]
    [InlineData("hello world", 11, "hello world")]
    [InlineData("a" + new string('b', 100), 5, "a...b...b...b...b...b...b...b...b...b...b...")]
    public void Truncate_ShouldReturnCorrectResult(string input, int maxLength, string expected)
    {
        var result = _processor.Truncate(input, maxLength);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, 0)]
    [InlineData("", 0)]
    [InlineData("hello", 0)]
    [InlineData("hello", 5)]
    [InlineData("hello world", 1)]
    [InlineData("hello world", 100)]
    public void CountWords_ShouldReturnCorrectWordCount(string input, int expected)
    {
        var result = _processor.CountWords(input);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("a", false)]
    [InlineData("racecar", true)]
    [InlineData("hello", false)]
    [InlineData("A man a plan a canal Panama", true)]
    [InlineData("Was it a car or a cat I saw", true)]
    [InlineData("Madam", true)]
    [InlineData("Hello World", false)]
    public void IsPalindrome_ShouldReturnCorrectResult(string input, bool expected)
    {
        var result = _processor.IsPalindrome(input);
        result.Should().Be(expected);
    }
}