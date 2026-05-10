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
    private readonly StringProcessor _sut = new();

    #region Truncate Tests

    [Theory]
    [InlineData(null, 5, "")]
    [InlineData("", 5, "")]
    [InlineData("Hi", 5, "Hi")]
    [InlineData("Hello", 5, "Hello")]
    [InlineData("Hello World", 5, "Hello...")]
    [InlineData("Test", 2, "Te...")]
    [InlineData("A", 1, "A")]
    [InlineData("AB", 1, "A...")]
    public void Truncate_ShouldReturnExpectedResult(string input, int maxLength, string expected)
    {
        var result = _sut.Truncate(input, maxLength);
        result.Should().Be(expected);
    }

    #endregion

    #region CountWords Tests

    [Theory]
    [InlineData(null, 0)]
    [InlineData("", 0)]
    [InlineData("   ", 0)]
    [InlineData("hello", 1)]
    [InlineData("hello world", 2)]
    [InlineData("one two three", 3)]
    [InlineData("  spaced   out  ", 2)]
    [InlineData("single", 1)]
    public void CountWords_ShouldReturnExpectedCount(string input, int expected)
    {
        var result = _sut.CountWords(input);
        result.Should().Be(expected);
    }

    #endregion

    #region IsPalindrome Tests

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("a", true)]
    [InlineData("racecar", true)]
    [InlineData("hello", false)]
    [InlineData("RaceCar", true)]
    [InlineData("A man a plan a canal Panama", true)]
    [InlineData("No lemon, no melon", false)] // Note: punctuation is not removed in implementation, so this is false
    [InlineData("abba", true)]
    [InlineData("abc", false)]
    public void IsPalindrome_ShouldReturnExpectedResult(string input, bool expected)
    {
        var result = _sut.IsPalindrome(input);
        result.Should().Be(expected);
    }

    #endregion
}