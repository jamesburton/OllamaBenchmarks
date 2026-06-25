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
    [InlineData(null, 10, null)]
    [InlineData("", 10, "")]
    [InlineData("short", 10, "short")]
    [InlineData("exact", 5, "exact")]
    [InlineData("longerthanmax", 5, "long...")]
    public void Truncate_NullOrEmptyOrShortOrEqualOrLong(string input, int maxLength, string expected)
    {
        var processor = new StringProcessor();
        var result = processor.Truncate(input, maxLength);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("word", 1)]
    [InlineData("hello world", 2)]
    [InlineData("   multiple   words   ", 3)]
    public void CountWords_NullOrEmptyOrSingleOrMultipleOrExtraSpaces(string input, int expected)
    {
        var processor = new StringProcessor();
        var result = processor.CountWords(input);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", false)]
    [InlineData("a", true)]
    [InlineData("madam", true)]
    [InlineData("hello", false)]
    [InlineData("RaceCar", true)]
    public void IsPalindrome_NullOrEmptyOrSingleOrPalindromeOrNonPalindromeOrMixedCase(string input, bool expected)
    {
        var processor = new
        {
            IsPalindrome = new StringProcessor().IsPalindrome
        };
        var result = processor.IsPalindrome(input);
        result.Should().Be(expected);
    }
}