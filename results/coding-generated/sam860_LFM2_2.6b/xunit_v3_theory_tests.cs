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
    [InlineData("hello", 5, "hel.")]
    [InlineData("hello world", 10, "hello world")] // maxLength > length
    [InlineData("a", 1, "a")]
    public void Truncate_ShouldReturnCorrectResult(string input, int maxLength, string expected)
    {
        var result = _processor.Truncate(input, maxLength);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, 10, 0)]
    [InlineData("", 10, 0)]
    [InlineData("hello world", 10, 5)] // split("hello world", " ", StringSplitOptions.RemoveEmptyEntries) => 2 words, length 5
    [InlineData("one two three", 10, 3)] // split("one two three", " ", StringSplitOptions.RemoveEmptyEntries) => 3 words, length 3
    [InlineData("   ", 10, 0)] // multiple spaces, no words
    [InlineData("singleword", 10, 1)] // single word
    public void CountWords_ShouldReturnCorrectWordCount(string input, int maxLength, int expected)
    {
        var result = _processor.CountWords(input);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, 10, false)]
    [InlineData("   ", 10, false)]
    [InlineData("racecar", 10, true)]
    [InlineData("hello", 10, false)]
    [InlineData("A man a plan a canal Panama", 10, true)] // case-insensitive palindrome
    [InlineData("Not a palindrome", 10, false)]
    public void IsPalindrome_ShouldReturnCorrectResult(string input, int maxLength, bool expected)
    {
        var result = _processor.IsPalindrome(input);
        result.Should().Be(expected);
    }
}