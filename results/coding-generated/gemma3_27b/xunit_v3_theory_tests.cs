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
    [Theory, InlineData(null, 10), InlineData("", 10), InlineData("hello", 10), InlineData("hello", 5), InlineData("hello world", 5)]
    public void Truncate_Should_ReturnCorrectResult(string input, int maxLength)
    {
        var processor = new StringProcessor();
        string result = processor.Truncate(input, maxLength);

        if (input is null)
        {
            result.Should().Be("");
        }
        else if (input == "")
        {
            result.Should().Be("");
        }
        else if (input.Length <= maxLength)
        {
            result.Should().Be(input);
        }
        else
        {
            result.Should().StartWith(input[..maxLength]);
            result.Should().EndWith("...");
            result.Should().HaveLength(maxLength + 3);
        }
    }

    [Theory, InlineData(null, 0), InlineData("", 0), InlineData("word", 1), InlineData("multiple words", 2), InlineData("  multiple   words  ", 2)]
    public void CountWords_Should_ReturnCorrectCount(string input, int expectedCount)
    {
        var processor = new StringProcessor();
        int result = processor.CountWords(input);
        result.Should().Be(expectedCount);
    }

    [Theory, InlineData(null, false), InlineData("", false), InlineData("a", true), InlineData("madam", true), InlineData("hello", false), InlineData("Race car", true)]
    public void IsPalindrome_Should_ReturnCorrectResult(string input, bool expectedResult)
    {
        var processor = new StringProcessor();
        bool result = processor.IsPalindrome(input);
        result.Should().Be(expectedResult);
    }
}