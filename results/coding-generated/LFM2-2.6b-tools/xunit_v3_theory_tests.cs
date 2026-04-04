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
    private readonly StringProcessor _processor = new();

    [Theory]
    [InlineData(null, 10, "")]
    [InlineData("", 10, "")]
    [InlineData("short", 5, "short")]
    [InlineData("exactly10", 10, "exactly10")]
    [InlineData("waylongstring", 10, "waylongstring")]
    public void Truncate_ShouldReturnCorrectResult(string input, int maxLength, string expected)
    {
        var result = _processor.Truncate(input, maxLength);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, 10, 0)]
    [InlineData("   ", 10, 1]]
    [InlineData("a", 5, 1]]
    [InlineData("exactly5", 5, 5)]
    [InlineData("waylongstring", 10, "waylongstring")]
    public void CountWords_ShouldReturnCorrectWordCount(string input, int maxLength, int expected)
    {
        var result = _processor.CountWords(input);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, 10, 0)]
    [InlineData("   ", 10, 1]]
    [InlineData("a", 5, 1)]
    [InlineData("exactly5", 5, 5)]
    [InlineData("waylongstring", 10, "waylongstring")]
    public void CountWords_ShouldHandleExtraSpaces(string input, int maxLength, int expected)
    {
        var result = _processor.CountWords(input);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, 10, false)]
    [InlineData("   ", 10, false)]
    [InlineData("a", 5, false)]
    [InlineData("aabb", 4, true)]
    [InlineData("racecar", 5, true)]
    [InlineData("hello world", 20, true)]
    public void IsPalindrome_ShouldReturnCorrectResult(string input, int maxLength, bool expected)
    {
        var result = _processor.IsPalindrome(input);
        result.Should().Be(expected);
    }
}