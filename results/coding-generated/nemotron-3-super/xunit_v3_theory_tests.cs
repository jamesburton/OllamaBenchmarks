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
    [InlineData("hi", 5, "hi")]
    [InlineData("hello", 5, "hello")]
    [InlineData("hello world", 5, "hello...")]
    public void Truncate_ReturnsCorrect(string input, int maxLength, string expected)
    {
        var sp = new StringProcessor();
        var result = sp.Truncate(input, maxLength);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, 0)]
    [InlineData("", 0)]
    [InlineData("single", 1)]
    [InlineData("two words", 2)]
    [InlineData("  extra   spaces   ", 2)]
    public void CountWords_ReturnsCorrect(string input, int expected)
    {
        var sp = new StringProcessor();
        var result = sp.CountWords(input);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("a", true)]
    [InlineData("Racecar", true)]
    [InlineData("hello", false)]
    [InlineData("Madam", true)]
    public void IsPalindrome_ReturnsCorrect(string input, bool expected)
    {
        var sp = new StringProcessor();
        var result = sp.IsPalindrome(input);
        result.Should().Be(expected);
    }
}