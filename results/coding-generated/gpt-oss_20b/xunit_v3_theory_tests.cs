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
        return input.Split(' ', System.StringSplitOptions.RemoveEmptyEntries).Length;
    }

    public bool IsPalindrome(string input)
    {
        if (string.IsNullOrEmpty(input)) return false;
        var cleaned = input.ToLowerInvariant().Replace(" ", "");
        return System.Linq.Enumerable.SequenceEqual(cleaned, System.Linq.Enumerable.Reverse(cleaned));
    }
}

public class StringProcessorTests
{
    private readonly StringProcessor _processor = new();

    [Theory]
    [InlineData(null, 5, "")]
    [InlineData("", 5, "")]
    [InlineData("abc", 5, "abc")]
    [InlineData("abcde", 5, "abcde")]
    [InlineData("abcdefgh", 5, "abcde...")]
    public void Truncate_ShouldReturnExpected(string input, int maxLength, string expected)
    {
        var result = _processor.Truncate(input, maxLength);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, 0)]
    [InlineData("", 0)]
    [InlineData("hello", 1)]
    [InlineData("hello world", 2)]
    [InlineData("  hello   world  ", 2)]
    public void CountWords_ShouldReturnExpected(string input, int expected)
    {
        var count = _processor.CountWords(input);
        count.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("a", true)]
    [InlineData("madam", true)]
    [InlineData("hello", false)]
    [InlineData("RaceCar", true)]
    [InlineData("A man a plan a canal Panama", true)]
    public void IsPalindrome_ShouldReturnExpected(string input, bool expected)
    {
        var result = _processor.IsPalindrome(input);
        result.Should().Be(expected);
    }
}