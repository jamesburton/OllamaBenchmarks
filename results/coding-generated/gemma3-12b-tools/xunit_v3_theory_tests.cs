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
    [Theory, InlineData(null, 10, "")]
    [Theory, InlineData("", 10, "")]
    [Theory, InlineData("hello", 5, "hello")]
    [Theory, InlineData("hello", 10, "hello")]
    [Theory, InlineData("hello world", 5, "hello...")]
    public void Truncate_Should_Return_Correct_String(string input, int maxLength, string expected)
    {
        var processor = new StringProcessor();
        processor.Truncate(input, maxLength).Should().Be(expected);
    }

    [Theory, InlineData(null, 0)]
    [Theory, InlineData("", 0)]
    [Theory, InlineData("hello", 1)]
    [Theory, InlineData("hello world", 2)]
    [Theory, InlineData("hello   world", 2)]
    public void CountWords_Should_Return_Correct_Count(string input, int expected)
    {
        var processor = new StringProcessor();
        processor.CountWords(input).Should().Be(expected);
    }

    [Theory, InlineData(null, false)]
    [Theory, InlineData("", false)]
    [Theory, InlineData("a", true)]
    [Theory, InlineData("madam", true)]
    [Theory, InlineData("hello", false)]
    [Theory, InlineData("Racecar", true)]
    public void IsPalindrome_Should_Return_Correct_Boolean(string input, bool expected)
    {
        var processor = new StringProcessor();
        processor.IsPalindrome(input).Should().Be(expected);
    }
}