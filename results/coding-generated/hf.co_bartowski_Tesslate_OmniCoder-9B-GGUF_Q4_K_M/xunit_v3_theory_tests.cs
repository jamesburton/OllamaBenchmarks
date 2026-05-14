public class StringProcessor
{
    public string Truncate(string input, int maxLength)
    [Fact]
    public void Truncate_NullInput_ReturnsEmptyString()
    {
        var sut = new StringProcessor();
        var result = sut.Truncate(null, 5);
        result.Should().Be("");
    }

    [Theory]
    [InlineData(null, 5)]
    [InlineData("", 5)]
    [InlineData("hi", 5)]
    [InlineData("hello", 5)]
    [InlineData("hello world", 5)]
    public void Truncate_Input_ReturnsCorrectResult(string input, int maxLength)
    {
        var sut = new StringProcessor();
        var result = sut.Truncate(input, maxLength);
        result.Should().Be(input.Length <= maxLength ? input : input[..maxLength] + "...");
    }

    [Fact]
    public void CountWords_NullInput_ReturnsZero()
    {
        var sut = new StringProcessor();
        var result = sut.CountWords(null);
        result.Should().Be(0);
    }

    [Fact]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("hello")]
    [InlineData("hello world")]
    [InlineData("hello   world")]
    public void CountWords_Input_ReturnsCorrectCount(string input)
    {
        var sut = new StringProcessor();
        var result = sut.CountWords(input);
        result.Should().Be(input.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length);
    }

    [Fact]
    public void IsPalindrome_NullInput_ReturnsFalse()
    {
        var sut = new StringProcessor();
        var result = sut.IsPalindrome(null);
        result.Should().Be(false);
    }

    [Fact]
    [InlineData("a")]
    [InlineData("ab")]
    [InlineData("aba")]
    [InlineData("abba")]
    [InlineData("racecar")]
    [InlineData("RaceCar")]
    [InlineData("hello")]
    [InlineData("Hello")]
    public void IsPalindrome_Input_ReturnsCorrectResult(string input)
    {
        var sut = new StringProcessor();
        var result = sut.IsPalindrome(input);
        result.Should().Be(input.ToLowerInvariant().Replace(" ", "").SequenceEqual(input.ToLowerInvariant().Replace(" ", "").Reverse()));
    }
}

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
    [Fact]
    public void Truncate_NullInput_ReturnsEmptyString()
    {
        var sut = new StringProcessor();
        var result = sut.Truncate(null, 5);
        result.Should().Be("");
    }

    [Theory]
    [InlineData(null, 5)]
    [InlineData("", 5)]
    [InlineData("hi", 5)]
    [InlineData("hello", 5)]
    [InlineData("hello world", 5)]
    public void Truncate_Input_ReturnsCorrectResult(string input, int maxLength)
    {
        var sut = new StringProcessor();
        var result = sut.Truncate(input, maxLength);
        result.Should().Be(input.Length <= maxLength ? input : input[..maxLength] + "...");
    }

    [Fact]
    public void CountWords_NullInput_ReturnsZero()
    {
        var sut = new StringProcessor();
        var result = sut.CountWords(null);
        result.Should().Be(0);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("hello")]
    [InlineData("hello world")]
    [InlineData("hello   world")]
    public void CountWords_Input_ReturnsCorrectCount(string input)
    {
        var sut = new StringProcessor();
        var result = sut.CountWords(input);
        result.Should().Be(input.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length);
    }

    [Fact]
    public void IsPalindrome_NullInput_ReturnsFalse()
    {
        var sut = new StringProcessor();
        var result = sut.IsPalindrome(null);
        result.Should().Be(false);
    }

    [Theory]
    [InlineData("a")]
    [InlineData("ab")]
    [InlineData("aba")]
    [InlineData("abba")]
    [InlineData("racecar")]
    [InlineData("RaceCar")]
    [InlineData("hello")]
    [InlineData("Hello")]
    public void IsPalindrome_Input_ReturnsCorrectResult(string input)
    {
        var sut = new StringProcessor();
        var result = sut.IsPalindrome(input);
        result.Should().Be(input.ToLowerInvariant().Replace(" ", "").SequenceEqual(input.ToLowerInvariant().Replace(" ", "").Reverse()));
    }
}