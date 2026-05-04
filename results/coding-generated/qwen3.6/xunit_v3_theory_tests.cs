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
    private readonly StringProcessor _sut = new();

    [Theory]
    [InlineData(null, 5)]
    [InlineData("", 5)]
    [InlineData("hi", 5)]
    [InlineData("hello", 5)]
    [InlineData("hello world", 5)]
    public void Truncate_should_handle_various_inputs(string input, int maxLength)
    {
        var result = _sut.Truncate(input, maxLength);

        if (input is null)
        {
            result.Should().Be("");
        }
        else if (input.Length <= maxLength)
        {
            result.Should().Be(input);
        }
        else
        {
            result.Should().HaveLength(maxLength + 3);
            result.Should().EndWith(".");
            result.Should().StartWith(input[..maxLength]);
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("hello")]
    [InlineData("hello world")]
    [InlineData("  hello   world  ")]
    public void CountWords_should_handle_various_inputs(string input)
    {
        var result = _sut.CountWords(input);

        if (string.IsNullOrWhiteSpace(input))
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
    [InlineData("racecar")]
    [InlineData("hello")]
    [InlineData("RaceCar")]
    public void IsPalindrome_should_handle_various_inputs(string input)
    {
        var result = _sut.IsPalindrome(input);

        if (string.IsNullOrEmpty(input))
        {
            result.Should().BeFalse();
        }
        else
        {
            var cleaned = input.ToLowerInvariant().Replace(" ", "");
            var expected = cleaned.SequenceEqual(cleaned.Reverse());
            result.Should().Be(expected);
        }
    }
}