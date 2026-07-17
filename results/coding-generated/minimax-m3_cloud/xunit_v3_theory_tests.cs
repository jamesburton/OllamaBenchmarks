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

    // -------------------- Truncate --------------------

    [Theory]
    [InlineData(null, 5, "")]
    [InlineData("", 5, "")]
    [InlineData("hi", 10, "hi")]
    [InlineData("hello", 5, "hello")]
    [InlineData("hello world", 5, "hello...")]
    public void Truncate_ReturnsExpectedValue(string input, int maxLength, string expected)
    {
        var result = _sut.Truncate(input, maxLength);

        result.Should().Be(expected);
    }

    [Fact]
    public void Truncate_NullInput_ReturnsEmptyString()
    {
        var result = _sut.Truncate(null, 10);

        result.Should().Be("");
    }

    // -------------------- CountWords --------------------

    [Theory]
    [InlineData(null, 0)]
    [InlineData("", 0)]
    [InlineData("hello", 1)]
    [InlineData("hello world", 2)]
    [InlineData("the quick brown fox", 4)]
    [InlineData("the   quick   brown   fox", 4)]
    [InlineData("   ", 0)]
    public void CountWords_ReturnsExpectedCount(string input, int expected)
    {
        var result = _sut.CountWords(input);

        result.Should().Be(expected);
    }

    // -------------------- IsPalindrome --------------------

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("a", true)]
    [InlineData("racecar", true)]
    [InlineData("hello", false)]
    [InlineData("RaceCar", true)]
    [InlineData("A man a plan a canal Panama", false)] // contains spaces, but is actually a palindrome
    public void IsPalindrome_ReturnsExpectedResult(string input, bool expected)
    {
        var result = _sut.IsPalindrome(input);

        result.Should().Be(expected);
    }

    [Fact]
    public void IsPalindrome_PhraseWithSpaces_ReturnsTrueWhenPalindromic()
    {
        var result = _sut.IsPalindrome("never odd or even");

        result.Should().BeTrue();
    }
}