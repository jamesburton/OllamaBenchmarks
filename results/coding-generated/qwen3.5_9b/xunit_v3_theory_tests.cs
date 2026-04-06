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
    public void Truncate_ReturnsCorrectResult(string input, int maxLength)
    {
        var result = _sut.Truncate(input, maxLength);

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
    [InlineData("  hello   world  ")]
    public void CountWords_ReturnsCorrectResult(string input)
    {
        var result = _sut.CountWords(input);

        if (input is null || string.IsNullOrEmpty(input))
        {
            result.Should().Be(0);
        }
        else
        {
            result.Should().BeGreaterThan(0);
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("racecar")]
    [InlineData("hello")]
    [InlineData("RaceCar")]
    public void IsPalindrome_ReturnsCorrectResult(string input)
    {
        var result = _sut.IsPalindrome(input);

        if (input is null || string.IsNullOrEmpty(input))
        {
            result.Should().Be(false);
        }
        else if (input.Length == 1)
        {
            result.Should().Be(true);
        }
        else if (input.ToLowerInvariant() == input.Reverse().ToString())
        {
            result.Should().Be(true);
        }
        else
        {
            result.Should().Be(false);
        }
    }

    [Theory]
    [InlineData("racecar")]
    [InlineData("RaceCar")]
    [InlineData("A man a plan a canal Panama")]
    public void IsPalindrome_ReturnsTrueForValidPalindromes(string input)
    {
        var result = _sut.IsPalindrome(input);

        result.Should().Be(true);
    }

    [Theory]
    [InlineData("hello")]
    [InlineData("Hello World")]
    [InlineData("abc")]
    public void IsPalindrome_ReturnsFalseForNonPalindromes(string input)
    {
        var result = _sut.IsPalindrome(input);

        result.Should().Be(false);
    }
}