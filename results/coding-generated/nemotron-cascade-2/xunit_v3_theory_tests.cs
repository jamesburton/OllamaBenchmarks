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
    [InlineData(null, 5, "")]
    [InlineData("", 3, "")]
    [InlineData("hi", 5, "hi")]
    [InlineData("hello", 5, "hello")]
    [InlineData("welcome", 3, "wel...")]
    public void Truncate_ReturnsExpected(string input, int maxLength, string expected)
    {
        var result = _sut.Truncate(input, maxLength);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, 0)]
    [InlineData("", 0)]
    [InlineData("single", 1)]
    [InlineData("word1 word2", 2)]
    [InlineData("  extra   spaces  ", 2)]
    public void CountWords_ReturnsExpected(string input, int expected)
    {
        var result = _sut.CountWords(input);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("a", true)]
    [InlineData("madam", true)]
    [InlineData("hello", false)]
    [InlineData("RaceCar", true)]
    [InlineData("A man a plan a canal Panama", true)]
    [InlineData("Was it a car or a cat I saw", true)]
    [InlineData("Not a palindrome", false)]
    public void IsPalindrome_ReturnsExpected(string input, bool expected)
    {
        var result = _sut.IsPalindrome(input);
        result.Should().Be(expected);
    }
}