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
    [InlineData(null, 5, "")]
    [InlineData("", 5, "")]
    [InlineData("Hello", 10, "Hello")]
    [InlineData("Hello World", 5, "Hello...")]
    public void Truncate_ReturnsExpectedString(string input, int maxLength, string expected)
    {
        // Arrange
        var processor = new StringProcessor();

        // Act
        var result = processor.Truncate(input, maxLength);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, 0)]
    [InlineData("", 0)]
    [InlineData("Hello", 1)]
    [InlineData("Hello World", 2)]
    [InlineData("   Hello   World  ", 2)]
    public void CountWords_ReturnsExpectedCount(string input, int expected)
    {
        // Arrange
        var processor = new StringProcessor();

        // Act
        var result = processor.CountWords(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("a", true)]
    [InlineData("Madam", true)]
    [InlineData("Hello", false)]
    [InlineData("A man a plan a canal Panama", true)]
    public void IsPalindrome_ReturnsExpectedResult(string input, bool expected)
    {
        // Arrange
        var processor = new StringProcessor();

        // Act
        var result = processor.IsPalindrome(input);

        // Assert
        result.Should().Be(expected);
    }
}