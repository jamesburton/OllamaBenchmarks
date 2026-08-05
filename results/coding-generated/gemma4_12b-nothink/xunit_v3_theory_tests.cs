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

    [Theory]
    [InlineData(null, 5, "")]
    [InlineData("", 5, "")]
    [InlineData("short", 10, "short")]
    [InlineData("exact", 5, "exact")]
    [InlineData("verylongstring", 4, "very...")]
    public void Truncate_ShouldReturnExpectedString(string input, int maxLength, string expected)
    {
        var result = _sut.Truncate(input, maxLength);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, 0)]
    [InlineData("", 0)]
    [InlineData("   ", 0)]
    [InlineData("Hello", 1)]
    [InlineData("Hello World", 2)]
    [InlineData("  Multiple   Spaces  ", 3)]
    public void CountWords_ShouldReturnCorrectCount(string input, int expected)
    {
        var result = _sut.CountWords(input);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("a", true)]
    [InlineData("racecar", true)]
    [InlineData("hello", false)]
    [InlineData("Race Car", true)]
    public void IsPalindrome_ShouldReturnExpectedResult(string input, bool expected)
    {
        var result = _sut.IsPalindrome(input);
        result.Should().Be(expected);
    }
}