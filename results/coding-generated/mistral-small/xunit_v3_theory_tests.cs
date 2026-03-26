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
    private readonly StringProcessor _processor;

    public StringProcessorTests()
    {
        _processor = new StringProcessor();
    }

    [Theory]
    [InlineData(null, 5, "")]
    [InlineData("", 5, "")]
    [InlineData("short", 10, "short")]
    [InlineData("exact", 5, "exact")]
    [InlineData("longertext", 5, "long...")]
    public void Truncate_ShouldReturnCorrectTruncatedString(string input, int maxLength, string expected)
    {
        var result = _processor.Truncate(input, maxLength);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, 0)]
    [InlineData("", 0)]
    [InlineData("single", 1)]
    [InlineData("multiple words", 2)]
    [InlineData(" extra spaces ", 1)]
    public void CountWords_ShouldReturnCorrectWordCount(string input, int expected)
    {
        var result = _processor.CountWords(input);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("a", true)]
    [InlineData("racecar", true)]
    [InlineData("hello", false)]
    [InlineData("A man a plan a canal Panama", true)]
    public void IsPalindrome_ShouldReturnCorrectPalindromeStatus(string input, bool expected)
    {
        var result = _processor.IsPalindrome(input);
        result.Should().Be(expected);
    }
}