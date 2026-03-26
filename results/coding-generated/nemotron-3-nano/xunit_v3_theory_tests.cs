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

    [Theory]
    [InlineData(null, 5, "")]
    [InlineData("", 5, "")]
    [InlineData("hello", 3, "hel...")]
    [InlineData("hello", 5, "hello")]
    [InlineData("hello", 2, "he...")]
    public void Truncate_Should_HandleVariousInputs(string input, int maxLength, string expected)
    {
        var result = _processor.Truncate(input, maxLength);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, 0)]
    [InlineData("", 0)]
    [InlineData("hello", 1)]
    [InlineData("a b c", 3)]
    [InlineData("  multiple   spaces  ", 7)]
    public void CountWords_Should_ReturnCorrectCount(string input, int expected)
    {
        var result = _processor.CountWords(input);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("a", true)]
    [InlineData("Level", true)]
    [InlineData("RaceCar", true)]
    [InlineData("hello", false)]
    [InlineData("palindrome", false)]
    public void IsPalindrome_Should_RecognizePalindromes(string input, bool expected)
    {
        var result = _processor.IsPalindrome(input);
        result.Should().Be(expected);
    }
}