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
    [InlineData("hi")]
    [InlineData("hello")]
    [InlineData("hello world")]
    public void Truncate_WhenInputIsNullOrEmptyOrShort_ReturnsInputOrEmpty(string input, int maxLength, string expected)
    {
        var processor = new StringProcessor();
        var result = processor.Truncate(input, maxLength);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, 0)]
    [InlineData("", 0)]
    [InlineData("hello", 5)]
    [InlineData("hello", 10)]
    [InlineData("hello world", 5)]
    public void Truncate_WhenInputIsNullOrEmptyOrShort_ReturnsInputOrEmpty(string input, int maxLength, string expected)
    {
        var processor = new StringProcessor();
        var result = processor.Truncate(input, maxLength);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("hello")]
    [InlineData("hello world")]
    public void CountWords_WhenInputIsNullOrEmptyOrHasWords_ReturnsCorrectCount(string input, int expected)
    {
        var processor = new StringProcessor();
        var result = processor.CountWords(input);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, 0)]
    [InlineData("", 0)]
    [InlineData("hello", 1)]
    [InlineData("hello world", 2)]
    [InlineData("hello   world", 2)]
    public void CountWords_WhenInputIsNullOrEmptyOrHasWords_ReturnsCorrectCount(string input, int expected)
    {
        var processor = new StringProcessor();
        var result = processor.CountWords(input);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("racecar")]
    [InlineData("RaceCar")]
    [InlineData("hello")]
    public void IsPalindrome_WhenInputIsNullOrEmptyOrPalindrome_ReturnsCorrectResult(string input, bool expected)
    {
        var processor = new StringProcessor();
        var result = processor.IsPalindrome(input);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("a", true)]
    [InlineData("racecar", true)]
    [InlineData("RaceCar", true)]
    [InlineData("hello", false)]
    public void IsPalindrome_WhenInputIsNullOrEmptyOrPalindrome_ReturnsCorrectResult(string input, bool expected)
    {
        var processor = new StringProcessor();
        var result = processor.IsPalindrome(input);
        result.Should().Be(expected);
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
    [Theory]
    [InlineData(null, 10, "")]
    [InlineData("", 10, "")]
    [InlineData("hi", 10, "hi")]
    [InlineData("hello", 5, "hello")]
    [InlineData("hello world", 5, "hello..")]
    public void Truncate_WhenInputIsNullOrEmptyOrShort_ReturnsInputOrEmpty(string input, int maxLength, string expected)
    {
        var processor = new StringProcessor();
        var result = processor.Truncate(input, maxLength);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, 0)]
    [InlineData("", 0)]
    [InlineData("hello", 1)]
    [InlineData("hello world", 2)]
    [InlineData("hello   world", 2)]
    public void CountWords_WhenInputIsNullOrEmptyOrHasWords_ReturnsCorrectCount(string input, int expected)
    {
        var processor = new StringProcessor();
        var result = processor.CountWords(input);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("a", true)]
    [InlineData("racecar", true)]
    [InlineData("RaceCar", true)]
    [InlineData("hello", false)]
    public void IsPalindrome_WhenInputIsNullOrEmptyOrPalindrome_ReturnsCorrectResult(string input, bool expected)
    {
        var processor = new StringProcessor();
        var result = processor.IsPalindrome(input);
        result.Should().Be(expected);
    }
}