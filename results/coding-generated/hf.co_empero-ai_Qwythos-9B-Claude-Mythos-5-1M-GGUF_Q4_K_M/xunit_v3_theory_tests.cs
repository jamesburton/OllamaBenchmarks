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
    [InlineData("hi", 2)]
    [InlineData("hello", 5)]
    [InlineData("hello world", 10)]
    public void Truncate_ReturnsCorrectResult(string input, int maxLength)
    {
        var sut = new StringProcessor();
        string expected = input is null ? "" : (input.Length <= maxLength ? input : input[..maxLength] + "...");
        sut.Truncate(input, maxLength).Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("hello")]
    [InlineData("one two three")]
    [InlineData("  hello   world  ")]
    public void CountWords_ReturnsCorrectResult(string input)
    {
        var sut = new StringProcessor();
        int expected = string.IsNullOrWhiteSpace(input) ? 0 : input.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        sut.CountWords(input).Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("racecar")]
    [InlineData("RaceCar")]
    [InlineData("hello")]
    public void IsPalindrome_ReturnsCorrectResult(string input)
    {
        var sut = new StringProcessor();
        bool expected = string.IsNullOrEmpty(input) ? false : (input.ToLowerInvariant().Replace(" ", "") == input.ToLowerInvariant().Replace(" ", "").Reverse().ToString());
        sut.IsPalindrome(input).Should().Be(expected);
    }
}