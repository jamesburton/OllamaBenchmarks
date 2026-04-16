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
    [InlineData(null, 10)]
    [InlineData("", 10)]
    [InlineData("hi", 10)]
    [InlineData("hello", 5)]
    [InlineData("hello world", 5)]
    public void Truncate_Should_Return_Correct_Output(string input, int maxLength)
    {
        var sut = new StringProcessor();
        var result = sut.Truncate(input, maxLength);

        if (input is null)
        {
            result.Should().Be("");
        }
        else if (string.IsNullOrEmpty(input))
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
    [InlineData("  hello  world  ")]
    public void CountWords_Should_Return_Correct_Count(string input)
    {
        var sut = new StringProcessor();
        var result = sut.CountWords(input);

        if (input is null || string.IsNullOrWhiteSpace(input))
        {
            result.Should().Be(0);
        }
        else
        {
            var expected = input.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            result.Should().Be(expected);
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("A man a plan a canal Panama")]
    [InlineData("hello world")]
    [InlineData("Race a car")]
    public void IsPalindrome_Should_Return_Correct_Result(string input)
    {
        var sut = new StringProcessor();
        var result = sut.IsPalindrome(input);

        if (input is null || string.IsNullOrEmpty(input))
        {
            result.Should().Be(false);
        }
        else if (input.Length == 1)
        {
            result.Should().Be(true);
        }
        else
        {
            var isPal = input.ToLowerInvariant().Replace(" ", "")
                .SequenceEqual(input.ToLowerInvariant().Replace(" ", "").Reverse());
            result.Should().Be(isPal);
        }
    }
}