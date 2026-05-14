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
    [InlineData(null)]
    [InlineData("")]
    [InlineData("short")]
    [InlineData("12345")]
    [InlineData("123456")]
    public void Truncate_ShouldHandleVariousInputs(string input, int maxLength)
    {
        var processor = new StringProcessor();

        var result = processor.Truncate(input, maxLength);

        if (input is null || input == "")
        {
            result.Should().Be("");
        }
        else if (input.Length <= maxLength)
        {
            result.Should().Be(input);
        }
        else
        {
            result.Should().Be($"{input[..maxLength]}...");
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("word")]
    [InlineData("word1 word2")]
    [InlineData("  word1   word2  ")]
    public void CountWords_ShouldCountCorrectly(string input, int expected)
    {
        var processor = new StringProcessor();

        var result = processor.CountWords(input);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("madam")]
    [InlineData("hello")]
    [InlineData("RaceCar")]
    public void IsPalindrome_ShouldReturnCorrectResult(string input, bool expected)
    {
        var processor = new StringProcessor();

        var result = processor.IsPalindrome(input);

        result.Should().Be(expected);
    }
}