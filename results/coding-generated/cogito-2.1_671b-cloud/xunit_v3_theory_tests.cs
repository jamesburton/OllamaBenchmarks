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
    [InlineData("hello", 5, "hello")]
    [InlineData("hello", 10, "hello")]
    [InlineData("hello world", 5, "hello...")]
    [InlineData("hello world", 0, "...")]
    public void Truncate_WithVariousInputs_ReturnsExpectedResult(string input, int maxLength, string expected)
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
    [InlineData("hello", 1)]
    [InlineData("hello world", 2)]
    [InlineData("  extra   spaces  ", 2)]
    [InlineData("  multiple   words   here  ", 3)]
    public void CountWords_WithVariousInputs_ReturnsExpectedCount(string input, int expectedCount)
    {
        // Arrange
        var processor = new StringProcessor();

        // Act
        var result = processor.CountWords(input);

        // Assert
        result.Should().Be(expectedCount);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("a", true)]
    [InlineData("racecar", true)]
    [InlineData("hello", false)]
    [InlineData("RaceCar", true)]
    [InlineData("A man a plan a canal Panama", true)]
    [InlineData("not a palindrome", false)]
    public void IsPalindrome_WithVariousInputs_ReturnsExpectedResult(string input, bool expected)
    {
        // Arrange
        var processor = new StringProcessor();

        // Act
        var result = processor.IsPalindrome(input);

        // Assert
        result.Should().Be(expected);
    }
}