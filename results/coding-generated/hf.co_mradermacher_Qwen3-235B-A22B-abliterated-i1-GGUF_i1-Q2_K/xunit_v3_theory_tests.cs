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
    [InlineData("abc", 5, "abc")]
    [InlineData("abcde", 5, "abcde")]
    [InlineData("abcdef", 5, "abcde...")]
    public void Truncate_ShouldReturnExpectedResult(string input, int maxLength, string expected)
    {
        var processor = new StringProcessor();
        var result = processor.Truncate(input, maxLength);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, 0)]
    [InlineData("", 0)]
    [InlineData("test", 1)]
    [InlineData("hello world", 2)]
    [InlineData("   test   ", 1)]
    public void CountWords_ShouldReturnExpectedCount(string input, int expectedCount)
    {
        var processor = new StringProcessor();
        var result = processor.CountWords(input);
        result.Should().Be(expectedCount);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("a", true)]
    [InlineData("racecar", true)]
    [InlineData("RaceCar", true)]
    [InlineData("hello", false)]
    [InlineData("A man a plan a canal Panama", true)]
    public void IsPalindrome_ShouldReturnExpectedBool(string input, bool expected)
    {
        var processor = new StringProcessor();
        var result = processor.IsPalindrome(input);
        result.Should().Be(expected);
    }
}