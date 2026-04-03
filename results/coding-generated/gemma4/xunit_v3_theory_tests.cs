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
    [InlineData(null, 5, "")] // Null input
    [InlineData("", 5, "")]    // Empty string
    [InlineData("abc", 5, "abc")] // Shorter than max
    [InlineData("abc", 3, "abc")] // Equal to max
    [InlineData("abcdef", 3, "abc...")] // Longer than max
    [InlineData("abc", 0, "...")] // Max length 0 (should truncate to "" + "...")
    public void Truncate_ShouldHandleVariousLengths(string input, int maxLength, string expected)
    {
        var result = _processor.Truncate(input, maxLength);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, 0)] // Null input
    [InlineData("", 0)]    // Empty string
    [InlineData("   ", 0)] // Only whitespace
    [InlineData("hello", 1)] // Single word
    [InlineData("hello world", 2)] // Multiple words
    [InlineData("  leading space", 2)] // Leading/trailing spaces
    [InlineData("hello   world", 2)] // Extra spaces
    public void CountWords_ShouldCountWordsCorrectly(string input, int expectedCount)
    {
        var result = _processor.CountWords(input);
        result.Should().Be(expectedCount);
    }

    [Theory]
    [InlineData(null, false)] // Null input
    [InlineData("", false)]    // Empty string
    [InlineData("a", true)]    // Single character
    [InlineData("racecar", true)] // Simple palindrome
    [InlineData("RaceCar", true)] // Mixed case palindrome
    [InlineData("A man a plan a canal panama", true)] // Palindrome with spaces
    [InlineData("hello", false)] // Non-palindrome
    [InlineData("abcde", false)] // Non-palindrome
    public void IsPalindrome_ShouldDeterminePalindromeStatus(string input, bool expected)
    {
        var result = _processor.IsPalindrome(input);
        result.Should().Be(expected);
    }
}
```