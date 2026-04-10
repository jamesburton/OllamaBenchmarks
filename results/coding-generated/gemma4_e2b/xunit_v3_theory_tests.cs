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

using Xunit;
using AwesomeAssertions;

public class StringProcessorTests
{
    private readonly StringProcessor _processor = new StringProcessor();

    // --- Truncate Tests ---

    [Theory]
    [InlineData(null, 10, "")] // Null input
    [InlineData("", 10, "")]  // Empty string
    [InlineData("short", 10, "short")] // Shorter than max
    [InlineData("exact", 5, "exact")] // Equal to max
    [InlineData("longer string", 5, "longer...")] // Longer than max
    [InlineData("test", 3, "tes...")] // Longer than max, testing truncation logic
    public void Truncate_VariousInputs_ShouldHandleCases(string input, int maxLength, string expected)
    {
        string actual = _processor.Truncate(input, maxLength);

        if (input == null)
        {
            actual.Should().Be("");
        }
        else if (input.Length <= maxLength)
        {
            actual.Should().Be(input);
        }
        else
        {
            actual.Should().EndWith("...");
            actual.Should().HaveLength(maxLength + 3); // Length of substring + "..."
        }
    }

    // --- CountWords Tests ---

    [Theory]
    [InlineData(null, 0)] // Null input
    [InlineData("", 0)]  // Empty string
    [InlineData("single", 1)] // Single word
    [InlineData("two words", 2)] // Multiple words
    [InlineData("  extra spaces", 2)] // Extra spaces
    [InlineData("one  two   three", 3)] // Multiple spaces
    [InlineData("   ", 0)] // Only spaces
    public void CountWords_VariousInputs_ShouldReturnCorrectCount(string input, int expected)
    {
        int actual = _processor.CountWords(input);
        actual.Should().Be(expected);
    }

    // --- IsPalindrome Tests ---

    [Theory]
    [InlineData(null, false)] // Null input
    [InlineData("", false)]  // Empty string
    [InlineData("a", false)]  // Single character
    [InlineData("racecar", true)] // Palindrome
    [InlineData("level", true)] // Palindrome
    [InlineData("hello", false)] // Non-palindrome
    [InlineData("Madam", false)] // Non-palindrome (case sensitive check in implementation, but cleaned should handle it)
    [InlineData("A man a plan a canal Panama", true)] // Palindrome with spaces and mixed case
    [InlineData("No lemon no melon", true)] // Palindrome with spaces and mixed case
    public void IsPalindrome_VariousInputs_ShouldReturnCorrectResult(string input, bool expected)
    {
        bool actual = _processor.IsPalindrome(input);
        actual.Should().Be(expected);
    }
}