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
    [InlineData(null, 5, "")]
    [InlineData("", 5, "")]
    [InlineData("hi", 10, "hi")]
    [InlineData("hello", 5, "hello")]
    [InlineData("hello world", 5, "hello...")]
    public void Truncate_ReturnsExpected(string input, int maxLength, string expected)
    {
        var sut = new StringProcessor();
        var result = sut.Truncate(input, maxLength);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, 0)]
    [InlineData("", 0)]
    [InlineData("   ", 0)]
    [InlineData("hello", 1)]
    [InlineData("hello world", 2)]
    [InlineData("  hello   world  ", 2)]
    public void CountWords_ReturnsExpected(string input, int expected)
    {
        var sut = new StringProcessor();
        var result = sut.CountWords(input);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("a", true)]
    [InlineData("Racecar", true)]
    [InlineData("hello", false)]
    [InlineData("A man a plan a canal Panama", true)]
    public void IsPalindrome_ReturnsExpected(string input, bool expected)
    {
        var sut = new StringProcessor();
        var result = sut.IsPalindrome(input);
        result.Should().Be(expected);
    }
}