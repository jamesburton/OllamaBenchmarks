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

    // --- Truncate Tests ---

    [Theory]
    [InlineData(null, 5, "")] // null input
    [InlineData("", 5, "")]   // empty string
    [InlineData("short", 10, "short")] // shorter than max
    [InlineData("abcde", 5, "abcde")] // equal to max
    [InlineData("verylongstring", 5, "very...") // longer than max
    public void Truncate_ShouldTruncateOrReturnOriginal(string input, int maxLength, string expected)
    {
        var actual = _processor.Truncate(input, maxLength);
        actual.Should().Be(expected);
    }

    // --- CountWords Tests ---

    [Theory]
    [InlineData(null, 0)] // null input
    [InlineData("", 0)]   // empty string
    [InlineData("word", 1)] // single word
    [InlineData("hello world", 2)] // multiple words
    [InlineData("  leading spaces and   trailing spaces ", 3)] // extra spaces
    public void CountWords_ShouldCountWordsCorrectly(string input, int expectedCount)
    {
        var actual = _processor.CountWords(input);
        actual.Should().Be(expectedCount);
    }

    // --- IsPalindrome Tests ---

    [Theory]
    [InlineData(null, false)] // null input
    [InlineData("", false)]   // empty string
    [InlineData("a", true)]   // single character
    [InlineData("racecar", true)] // standard palindrome
    [InlineData("A man a plan a canal Panama", true)] // mixed case, spaces
    [InlineData("hello world", false)] // non-palindrome
    public void IsPalindrome_ShouldDeterminePalindromeStatus(string input, bool expected)
    {
        var actual = _processor.IsPalindrome(input);
        actual.Should().Be(expected);
    }
}
```