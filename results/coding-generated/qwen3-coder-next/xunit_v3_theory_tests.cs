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
    private readonly StringProcessor _processor = new();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("hi", 10)]
    [InlineData("hello", 5)]
    [InlineData("hello world", 5)]
    public void Truncate_ShouldHandleAllCases(string input, int maxLength = 5)
    {
        // Arrange
        string expected;
        if (input is null)
            expected = "";
        else if (input.Length <= maxLength)
            expected = input;
        else
            expected = input[..maxLength] + "...";

        // Act
        string result = _processor.Truncate(input, maxLength);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("hello", 1)]
    [InlineData("hello world", 2)]
    [InlineData("  hello   world  ", 2)]
    public void CountWords_ShouldHandleAllCases(string input, int expected = 0)
    {
        // Act
        int result = _processor.CountWords(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("a", true)]
    [InlineData("racecar", true)]
    [InlineData("hello", false)]
    [InlineData("RaceCar", true)]
    public void IsPalindrome_ShouldHandleAllCases(string input, bool expected = false)
    {
        // Act
        bool result = _processor.IsPalindrome(input);

        // Assert
        result.Should().Be(expected);
    }
}