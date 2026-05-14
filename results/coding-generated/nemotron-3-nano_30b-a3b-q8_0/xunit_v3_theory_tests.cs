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
    [Theory]
    [InlineData("", 0, "")]
    [InlineData("hello", 5, "hello")]
    [InlineData("hello", 3, "hel...")]
    [InlineData("hello", 0, "...")]
    [InlineData("this is a test", 2, "th...")]
    [InlineData("this is a test", 4, "this...")]
    [InlineData("this is a test", 10, "this is a test")]
    [InlineData(null, 5, "")]
    public void Truncate_ShouldHandleVariousInput(string input, int maxLength, string expected)
    {
        var result = processor.Truncate(input, maxLength);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("", 0)]
    [InlineData(null, 0)]
    [InlineData("  ", 0)]
    [InlineData("hello", 1)]
    [InlineData("hello world", 2)]
    [InlineData("a b   c", 3)]
    public void CountWords_ShouldHandleVariousInput(string input, int expected)
    {
        var result = processor.CountWords(input);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("a", true)]
    [InlineData("abba", true)]
    [InlineData("abc", false)]
    [InlineData("RaceCar", true)]
    [InlineData("Was it a car or a cat I saw", true)]
    [InlineData("Not a palindrome", false)]
    public void IsPalindrome_ShouldHandleVariousInput(string input, bool expected)
    {
        var result = processor.IsPalindrome(input);
        result.Should().Be(expected);
    }

    private readonly StringProcessor processor = new();
}