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
    [InlineData(null, 0, "")]
    [InlineData("", 5, "")]
    [InlineData("a", 5, "a")]
    [InlineData("abc", 3, "abc")]
    [InlineData("abcdef", 3, "abc...")]
    public void Truncate_VariousInputs(string input, int maxLength, string expected)
    {
        var processor = new StringProcessor();
        processor.Truncate(input, maxLength).Should().Be(expected);
    }

    [Theory]
    [InlineData(null, 0)]
    [InlineData("", 0)]
    [InlineData("hello", 1)]
    [InlineData("hello world", 2)]
    [InlineData("  hello   world  ", 2)]
    public void CountWords_VariousInputs(string input, int expected)
    {
        var processor = new StringProcessor();
        processor.CountWords(input).Should().Be(expected);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("a", true)]
    [InlineData("racecar", true)]
    [InlineData("hello", false)]
    [InlineData("Racecar", true)]
    public void IsPalindrome_VariousInputs(string input, bool expected)
    {
        var processor = new StringProcessor();
        processor.IsPalindrome(input).Should().Be(expected);
    }
}