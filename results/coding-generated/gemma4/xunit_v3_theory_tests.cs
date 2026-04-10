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
    [InlineData(null, 10, "")] // null input
    [InlineData("", 10, "")]    // empty string
    [InlineData("abc", 5, "abc")] // shorter than max
    [InlineData("abcd", 4, "abcd")] // equal to max
    [InlineData("abcdef", 3, "abc...")] // longer than max
    public void Truncate_ShouldHandleVariousLengths(string input, int maxLength, string expected)
    {
        var actual = _processor.Truncate(input, maxLength);
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData(null)] // null input
    [InlineData("")]    // empty string
    [InlineData("   ")] // only spaces
    [InlineData("singleword")] // single word
    [InlineData("hello world")] // multiple words
    [InlineData("  leading space and   multiple spaces ")] // extra spaces
    public void CountWords_ShouldCountWordsCorrectly(string input)
    {
        var actual = _processor.CountWords(input);
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData(null)] // null input
    [InlineData("")]    // empty string
    [InlineData("a")]   // single character
    [InlineData("racecar")] // palindrome
    [InlineData("hello")] // non-palindrome
    [InlineData("RaceCar")] // mixed case palindrome
    [InlineData("a b a")] // palindrome with spaces
    public void IsPalindrome_ShouldDeterminePalindromeStatus(string input)
    {
        var actual = _processor.IsPalindrome(input);
        actual.Should().Be(expected);
    }
}