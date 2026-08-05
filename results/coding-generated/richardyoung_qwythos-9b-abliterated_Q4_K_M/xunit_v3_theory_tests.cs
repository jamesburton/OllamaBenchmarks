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
    [InlineData("", 0)]
    [InlineData("hi", 2)]
    [InlineData("hello", 5)]
    [InlineData("world", 4)]
    public void Truncate_ReturnsCorrectResult(string input, int maxLength)
    {
        var result = _sut.Truncate(input, maxLength);

        if (input is null)
            result.Should().Be("");
        else if (string.IsNullOrEmpty(input))
            result.Should().Be("");
        else if (input.Length <= maxLength)
            result.Should().Be(input);
        else
            result.Should().StartWith(input[..maxLength]).And.EndWith("...");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("hello")]
    public void CountWords_ReturnsCorrectCount(string input)
    {
        var count = _sut.CountWords(input);

        if (input is null || string.IsNullOrEmpty(input))
            count.Should().Be(0);
        else
            count.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("racecar")]
    [InlineData("RaceCar")]
    [InlineData("hello world")]
    [InlineData("Hello World")]
    public void IsPalindrome_ReturnsCorrectResult(string input)
    {
        var result = _sut.IsPalindrome(input);

        if (input is null || string.IsNullOrEmpty(input))
            result.Should().BeFalse();
        else if (input.Length == 1)
            result.Should().BeTrue();
        else if (input.ToLowerInvariant() == input.Reverse().ToLowerInvariant())
            result.Should().BeTrue();
        else
            result.Should().BeFalse();
    }
}