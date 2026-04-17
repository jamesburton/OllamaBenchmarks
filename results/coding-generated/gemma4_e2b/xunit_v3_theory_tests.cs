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
    private readonly StringProcessor _processor = new StringProcessor();

    [Theory]
    [InlineData(null, 10, "")] // Null input
    [InlineData("", 10, "")]  // Empty string
    [InlineData("short", 10, "short")] // Shorter than max
    [InlineData("exact", 5, "exact")] // Equal to max
    [InlineData("longer string", 5, "longer...")] // Longer than max
    [InlineData("palindrome", 10, "palindrome")] // Palindrome
    [InlineData("hello", 10, "hello")] // Non-palindrome
    [InlineData("racecar", 10, "racecar")] // Palindrome
    [InlineData("A man a plan", 10, "a man a...")] // Palindrome with spaces (should be handled by cleaning)
    [InlineData("  multiple   spaces ", 10, "multiple...")] // Multiple spaces
    public void Truncate_VariousInputs_ShouldProduceCorrectOutput(string input, int maxLength, string expected)
    {
        string actual = _processor.Truncate(input, maxLength);

        // Special handling for null input based on implementation: returns ""
        if (input == null)
        {
            actual.Should().Be("");
        }
        else
        {
            actual.Should().Be(expected);
        }
    }

    [Theory]
    [InlineData(null, 0)] // Null input
    [InlineData("", 0)]  // Empty string
    [InlineData("word", 0, 1)] // Single word
    [InlineData("word1 word2", 0, 2)] // Multiple words
    [InlineData("  leading and trailing  ", 0, 4)] // Extra spaces
    [InlineData("single", 10, 1)] // Single word test
    [InlineData("one two three", 10, 3)] // Multiple words test
    public void CountWords_VariousInputs_ShouldReturnCorrectCount(string input, int expectedCount)
    {
        int actual = _processor.CountWords(input);
        actual.Should().Be(expectedCount);
    }

    [Theory]
    [InlineData(null)] // Null input
    [InlineData("")]  // Empty string
    [InlineData("a")]  // Single character
    [InlineData("level")] // Palindrome
    [InlineData("hello")] // Non-palindrome
    [InlineData("madam")] // Palindrome
    [InlineData("   ")] // Only spaces
    [InlineData("  a b  c", 10, 3)] // Palindrome with spaces (should be 3 words)
    public void IsPalindrome_VariousInputs_ShouldReturnCorrectBoolean(string input)
    {
        bool actual = _processor.IsPalindrome(input);

        // Based on implementation: null/empty returns false
        if (string.IsNullOrEmpty(input))
        {
            actual.Should().Be(false);
        }
        else
        {
            actual.Should().Be(true, $"Input: {input}");
        }
    }
}