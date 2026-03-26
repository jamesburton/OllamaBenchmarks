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
    [InlineData(null, 5)]
    [InlineData("", 0)]
    [InlineData("short", 10)]
    [InlineData("equal", 5)]
    [InlineData("longer than max", 5)]
    public void Truncate_ValidInputs_ReturnsExpected(string input, int maxLength)
    {
        var processor = new StringProcessor();
        string result = processor.Truncate(input, maxLength);

        if (input is null || input.Length <= maxLength)
            result.Should().Be(input);
        else
            result.Should().Be(input[..maxLength] + "...");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("single")]
    [InlineData("  extra spaces  ")]
    public void CountWords_NullOrEmpty_ReturnsZero(string input)
    {
        var processor = new StringProcessor();
        int result = processor.CountWords(input);

        result.Should().Be(0);
    }

    [Theory]
    [InlineData("one", 1)]
    [InlineData("two words", 2)]
    [InlineData("three   multiple   spaces", 3)]
    public void CountWords_MultipleWords_ReturnsCorrectCount(string input, int expected)
    {
        var processor = new StringProcessor();
        int result = processor.CountWords(input);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("a")]
    public void IsPalindrome_NullOrSingleChar_ReturnsFalse(string input)
    {
        var processor = new StringProcessor();
        bool result = processor.IsPalindrome(input);

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("racecar", true)]
    [InlineData("madam", true)]
    [InlineData("step on no pets", true)]
    public void IsPalindrome_Palindrome_ReturnsTrue(string input, bool expected)
    {
        var processor = new StringProcessor();
        bool result = processor.IsPalindrome(input);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("hello", false)]
    [InlineData("not a palindrome", false)]
    public void IsPalindrome_NonPalindrome_ReturnsFalse(string input, bool expected)
    {
        var processor = new StringProcessor();
        bool result = processor.IsPalindrome(input);

        result.Should().Be(expected);
    }
}