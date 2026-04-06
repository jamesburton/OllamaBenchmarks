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
    [InlineData("hello", 10, "hello")]
    [InlineData("1234567890", 10, "1234567890")]
    [InlineData("1234567890123", 5, "12345...")]
    public void Truncate_ReturnsExpected(string input, int maxLength, string expected)
    {
        var processor = new StringProcessor();
        var result = processor.Truncate(input, maxLength);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, 0)]
    [InlineData("", 0)]
    [InlineData("hello", 1)]
    [InlineData("a b c", 3)]
    [InlineData("  a   b  c  ", 3)]
    public void CountWords_ReturnsExpected(string input, int expected)
    {
        var processor = new StringProcessor();
        var result = processor.CountWords(input);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("a", true)]
    [InlineData("madam", true)]
    [InlineData("hello", false)]
    [InlineData("RaceCar", true)]
    public void IsPalindrome_ReturnsExpected(string input, bool expected)
    {
        var processor = new StringProcessor();
        var result = processor.IsPalindrome(input);
        result.Should().Be(expected);
    }
}