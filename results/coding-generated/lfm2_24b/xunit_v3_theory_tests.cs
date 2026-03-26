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
    [InlineData(null, 10, "")]
    [InlineData("", 10, "")]
    [InlineData("hello", 10, "hello")]
    [InlineData("hello", 5, "hello..")]
    [InlineData("hello world", 5, "hello..")]
    public void Truncate(string input, int maxLength, string expected)
    {
        var processor = new StringProcessor();
        processor.Truncate(input, maxLength).Should().Be(expected);
    }

    [Theory]
    [InlineData(null, 10, 0)]
    [InlineData("", 10, 0)]
    [InlineData("hello", 10, 1)]
    [InlineData("hello world", 10, 2)]
    [InlineData("a b c d e", 10, 5)]
    public void CountWords(string input, int expected)
    {
        var processor = new StringProcessor();
        processor.CountWords(input).Should().Be(expected);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("a", true)]
    [InlineData("hello", true)]
    [InlineData("A man a plan a canal Panama", true)]
    [InlineData("Hello World", false)]
    public void IsPalindrome(string input, bool expected)
    {
        var processor = new StringProcessor();
        processor.IsPalindrome(input).Should().Be(expected);
    }
}