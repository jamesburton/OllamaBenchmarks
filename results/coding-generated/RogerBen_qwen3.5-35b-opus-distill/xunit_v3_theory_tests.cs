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
    public void Truncate_WhenInputLengthIsLessThanOrEqualMaxLength_ReturnsInput(string input, int maxLength)
    {
        var processor = new StringProcessor();
        var result = processor.Truncate(input, maxLength);
        result.Should().Be(input);
    }

    [Theory]
    [InlineData(null, 5)]
    [InlineData("", 5)]
    [InlineData("hi", 5)]
    [InlineData("hello", 5)]
    [InlineData("hello world", 5)]
    public void Truncate_WhenInputLengthIsGreaterThanMaxLength_ReturnsTruncatedInput(string input, int maxLength)
    {
        var processor = new StringProcessor();
        var result = processor.Truncate(input, maxLength);
        result.Should().NotBe(input);
        result.Should().EndWith(".");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("hello")]
    [InlineData("hello world")]
    public void CountWords_WhenInputIsNullOrWhitespace_ReturnsZero(string input)
    {
        var processor = new StringProcessor();
        var result = processor.CountWords(input);
        result.Should().Be(0);
    }

    [Theory]
    [InlineData("hello")]
    [InlineData("hello world")]
    [InlineData("hello   world")]
    public void CountWords_WhenInputHasWords_ReturnsCorrectCount(string input)
    {
        var processor = new StringProcessor();
        var result = processor.CountWords(input);
        result.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("A")]
    [InlineData("racecar")]
    [InlineData("RaceCar")]
    [InlineData("A man a plan a canal Panama")]
    public void IsPalindrome_WhenInputIsPalindrome_ReturnsTrue(string input)
    {
        var processor = new StringProcessor();
        var result = processor.IsPalindrome(input);
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("hello")]
    [InlineData("Hello")]
    [InlineData("hello world")]
    public void IsPalindrome_WhenInputIsNotPalindrome_ReturnsFalse(string input)
    {
        var processor = new StringProcessor();
        var result = processor.IsPalindrome(input);
        result.Should().BeFalse();
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
    [InlineData(null)]
    [InlineData("")]
    [InlineData("hi")]
    [InlineData("hello")]
    [InlineData("hello world")]
    public void Truncate_WhenInputLengthIsLessThanOrEqualMaxLength_ReturnsInput(string input, int maxLength)
    {
        var processor = new StringProcessor();
        var result = processor.Truncate(input, maxLength);
        result.Should().Be(input);
    }

    [Theory]
    [InlineData(null, 5)]
    [InlineData("", 5)]
    [InlineData("hi", 5)]
    [InlineData("hello", 5)]
    [InlineData("hello world", 5)]
    public void Truncate_WhenInputLengthIsGreaterThanMaxLength_ReturnsTruncatedInput(string input, int maxLength)
    {
        var processor = new StringProcessor();
        var result = processor.Truncate(input, maxLength);
        result.Should().NotBe(input);
        result.Should().EndWith(".");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("hello")]
    [InlineData("hello world")]
    public void CountWords_WhenInputIsNullOrWhitespace_ReturnsZero(string input)
    {
        var processor = new StringProcessor();
        var result = processor.CountWords(input);
        result.Should().Be(0);
    }

    [Theory]
    [InlineData("hello")]
    [InlineData("hello world")]
    [InlineData("hello   world")]
    public void CountWords_WhenInputHasWords_ReturnsCorrectCount(string input)
    {
        var processor = new StringProcessor();
        var result = processor.CountWords(input);
        result.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("A")]
    [InlineData("racecar")]
    [InlineData("RaceCar")]
    [InlineData("A man a plan a canal Panama")]
    public void IsPalindrome_WhenInputIsPalindrome_ReturnsTrue(string input)
    {
        var processor = new StringProcessor();
        var result = processor.IsPalindrome(input);
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("hello")]
    [InlineData("Hello")]
    [InlineData("hello world")]
    public void IsPalindrome_WhenInputIsNotPalindrome_ReturnsFalse(string input)
    {
        var processor = new StringProcessor();
        var result = processor.IsPalindrome(input);
        result.Should().BeFalse();
    }
}