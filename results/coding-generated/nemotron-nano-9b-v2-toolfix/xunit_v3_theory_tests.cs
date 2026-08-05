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
    [InlineData("", 5)]
    [InlineData("hello", 5)]
    [InlineData("hello", 3)]
    [InlineData("hello world", 5)]
    public void Truncate_ShouldReturnCorrectResult(string input, int maxLength)
    {
        var processor = new StringProcessor();
        var result = processor.Truncate(input, maxLength);

        if (input == null)
        {
            result.Should().Be("");
        }
        else if (input.Length <= maxLength)
        {
            result.Should().Be(input);
        }
        else
        {
            result.Should().Be(input[..maxLength] + "...");
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("hello")]
    [InlineData("hello world")]
    [InlineData("   ")]
    public void CountWords_ShouldReturnCorrectCount(string input)
    {
        var processor = new StringProcessor();
        var result = processor.CountWords(input);

        if (input == null || string.IsNullOrWhiteSpace(input))
        {
            result.Should().Be(0);
        }
        else if (input == "hello")
        {
            result.Should().Be(1);
        }
        else if (input == "hello world")
        {
            result.Should().Be(2);
        }
        else if (input == "   ")
        {
            result.Should().Be(0);
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("abba")]
    [InlineData("abc")]
    [InlineData("A man a plan")]
    public void IsPalindrome_ShouldReturnCorrectResult(string input)
    {
        var processor = new StringProcessor();
        var result = processor.IsPalindrome(input);

        if (input == null || string.IsNullOrEmpty(input))
        {
            result.Should().Be(false);
        }
        else if (input == "a")
        {
            result.Should().Be(true);
        }
        else if (input == "abba")
        {
            result.Should().Be(true);
        }
        else if (input == "abc")
        {
            result.Should().Be(false);
        }
        else if (input == "A man a plan")
        {
            result.Should().Be(false);
        }
    }
}