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
    private readonly StringProcessor _processor = new();

    [Fact]
    public void Truncate_NullInput_ReturnsEmptyString()
    {
        var result = _processor.Truncate(null, 10);
        result.Should().Be("");
    }

    [Theory]
    [InlineData("", 10, "")]
    [InlineData("hello", 5, "hello")]
    [InlineData("hello", 3, "hel...")]
    [InlineData("hello world", 5, "hell...")]
    public void Truncate_ValidInputs_ReturnsExpected(string input, int maxLength, string expected)
    {
        var result = _processor.Truncate(input, maxLength);
        result.Should().Be(expected);
    }

    [Fact]
    public void CountWords_NullInput_ReturnsZero()
    {
        var result = _processor.CountWords(null);
        result.Should().Be(0);
    }

    [Theory]
    [InlineData("", 0)]
    [InlineData("hello", 1)]
    [InlineData("hello world", 2)]
    [InlineData("  hello   world  ", 2)]
    [InlineData("   ", 0)]
    public void CountWords_ValidInputs_ReturnsExpected(string input, int expected)
    {
        var result = _processor.CountWords(input);
        result.Should().Be(expected);
    }

    [Fact]
    public void IsPalindrome_NullInput_ReturnsFalse()
    {
        var result = _processor.IsPalindrome(null);
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("a", true)]
    [InlineData("racecar", true)]
    [InlineData("RaceCar", true)]
    [InlineData("hello", false)]
    [InlineData("A man a plan a canal Panama", true)]
    [InlineData("No lemon, no melon", true)]
    public void IsPalindrome_ValidInputs_ReturnsExpected(string input, bool expected)
    {
        var result = _processor.IsPalindrome(input);
        result.Should().Be(expected);
    }
}