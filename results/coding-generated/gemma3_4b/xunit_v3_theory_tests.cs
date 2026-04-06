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

using AwesomeAssertions;
using Xunit;

public class StringProcessorTests
{
    [Theory]
    [InlineData(null, 5)]
    [InlineData("", 5)]
    [InlineData("hello", 5)]
    [InlineData("hello world", 10)]
    [InlineData("hello world!", 15)]
    public void TestTruncate(string input, int maxLength)
    {
        var processor = new StringProcessor();
        var result = processor.Truncate(input, maxLength);
        result.Should().Be(input.Length <= maxLength ? input : input[..maxLength] + "...");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("", 0)]
    [InlineData("one", 1)]
    [InlineData("one two", 2)]
    [InlineData("one  two", 2)]
    [InlineData("one two three", 3)]
    public void TestCountWords(string input)
    {
        var processor = new StringProcessor();
        var result = processor.CountWords(input);
        result.Should().Be(input.IsNullOrWhiteSpace() ? 0 : input.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("madam")]
    [InlineData("racecar")]
    [InlineData("hello world")]
    public void TestIsPalindrome(string input)
    {
        var processor = new StringProcessor();
        var result = processor.IsPalindrome(input);
        result.Should().Be(input.IsNullOrEmpty() ? false : input.ToLowerInvariant().Replace(" ", "").SequenceEqual(input.ToLowerInvariant().Replace(" ", "").Reverse()));
    }
}