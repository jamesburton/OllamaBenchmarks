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
    [InlineData("hello", 5)]
    [InlineData("hello world", 5)]
    public void Truncate(string input, int maxLength)
    {
        var sut = new StringProcessor();
        var result = sut.Truncate(input, maxLength);
        result.Should().Be(input.Length <= maxLength ? input : input[..maxLength] + "...");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("hello")]
    [InlineData("hello world")]
    [InlineData("hello   world")]
    public void CountWords(string input)
    {
        var sut = new StringProcessor();
        var result = sut.CountWords(input);
        result.Should().Be(input.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("racecar")]
    [InlineData("hello")]
    [InlineData("RaceCar")]
    public void IsPalindrome(string input)
    {
        var sut = new StringProcessor();
        var result = sut.IsPalindrome(input);
        result.Should().Be(input.Length <= 1 || input.ToLowerInvariant().Replace(" ", "").SequenceEqual(input.ToLowerInvariant().Replace(" ", "").Reverse()));
    }
}