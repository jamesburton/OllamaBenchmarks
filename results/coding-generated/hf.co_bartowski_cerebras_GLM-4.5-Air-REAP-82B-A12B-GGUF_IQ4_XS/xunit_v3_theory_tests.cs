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
    [InlineData("", 3, "")]
    [InlineData("abc", 5, "abc")]
    [InlineData("hello", 5, "hello")]
    [InlineData("longerstring", 4, "l...")]
    public void Truncate_ReturnsCorrectValue(string input, int maxLength, string expected)
    {
        var processor = new StringProcessor();
        string result = processor.Truncate(input, maxLength);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, 0)]
    [InlineData("", 0)]
    [InlineData("word", 1)]
    [InlineData("multiple words here", 4)]
    [InlineData("extra   spaces   between", 3)]
    public void CountWords_ReturnsCorrectCount(string input, int expected)
    {
        var processor = new StringProcessor();
        int result = processor.CountWords(input);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("a", true)]
    [InlineData("MaDam", true)]
    [InlineData("racecar", true)]
    [InlineData("hello", false)]
    public void IsPalindrome_ReturnsCorrectValue(string input, bool expected)
    {
        var processor = new StringProcessor();
        bool result = processor.IsPalindrome(input);
        result.Should().Be(expected);
    }
}