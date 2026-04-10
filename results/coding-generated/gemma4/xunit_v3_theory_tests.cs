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
    [InlineData(null, 5, "")] // null input
    [InlineData("", 5, "")]   // empty string
    [InlineData("short", 10, "short")] // shorter than max
    [InlineData("abcde", 5, "abcde")] // equal to max
    [InlineData("toolongstring", 5, "tool...")] // longer than max
    public void Truncate_ShouldHandleVariousInputs(string input, int maxLength, string expected)
    {
        var processor = new StringProcessor();
        var actual = processor.Truncate(input, maxLength);
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, 0)] // null input
    [InlineData("", 0)]   // empty string
    [InlineData("word", 1)] // single word
    [InlineData("hello world", 2)] // multiple words
    [InlineData("  multiple   spaces ", 3)] // extra spaces
    public void CountWords_ShouldCountWordsCorrectly(string input, int expectedCount)
    {
        var processor = new StringProcessor();
        var actual = processor.CountWords(input);
        actual.Should().Be(expectedCount);
    }

    [Theory]
    [InlineData(null, false)] // null input
    [InlineData("", false)]   // empty string
    [InlineData("a", true)]   // single character
    [InlineData("racecar", true)] // palindrome
    [InlineData("hello", false)] // non-palindrome
    [InlineData("A man a plan a canal Panama", true)] // palindrome with mixed case and spaces
    public void IsPalindrome_ShouldDetectPalindromes(string input, bool expected)
    {
        var processor = new StringProcessor();
        var actual = processor.IsPalindrome(input);
        actual.Should().Be(expected);
    }
}