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
    [InlineData(null, 0)]
    [InlineData("", 0)]
    [InlineData("a", 1)]
    [InlineData("abc", 3)]
    [InlineData("abcd", 4)]
    [InlineData("abcdef", 5)]
    public void Truncate_ShouldReturnCorrectlyTruncatedString(string input, int maxLength)
    {
        var processor = new StringProcessor();
        string result = processor.Truncate(input, maxLength);

        if (input is null) 
            result.Should().Be("");
        else
        {
            if (maxLength <= 0)
                result.Should().Be("");
            else if (input.Length <= maxLength)
                result.Should().Be(input);
            else
                result.Should().HaveLength(maxLength + 3).And.StartWith(input[..maxLength]);
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("hello")]
    [InlineData("one two three")]
    [InlineData("   one   two   three   ")]
    public void CountWords_ShouldReturnCorrectWordCount(string input)
    {
        var processor = new StringProcessor();
        int result = processor.CountWords(input);

        if (input is null || string.IsNullOrWhiteSpace(input))
            result.Should().Be(0);
        else
        {
            switch (input.Trim())
            {
                case "hello":
                    result.Should().Be(1);
                    break;
                case "one two three":
                    result.Should().Be(3);
                    break;
                default:
                    throw new Xunit.Sdk.XunitException($"Unexpected input: {input}");
            }
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("A man a plan a canal Panama")]
    [InlineData("racecar")]
    [InlineData("Hello World")]
    public void IsPalindrome_ShouldReturnCorrectResult(string input)
    {
        var processor = new StringProcessor();
        bool result = processor.IsPalindrome(input);

        if (input is null || string.IsNullOrEmpty(input))
            result.Should().BeFalse();

        switch (input?.Trim())
        {
            case "a":
                result.Should().BeTrue();
                break;
            case "A man a plan a canal Panama":
                result.Should().BeTrue();
                break;
            case "racecar":
                result.Should().BeTrue();
                break;
            default:
                result.Should().BeFalse();
                break;
        }
    }
}