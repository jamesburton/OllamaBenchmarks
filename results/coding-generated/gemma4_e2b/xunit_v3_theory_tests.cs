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
    [InlineData("", 5, "")]  // Empty string
    [InlineData("short", 10, "short")] // Shorter than max
    [InlineData("exact", 5, "exact")] // Equal to max
    [InlineData("longerstring", 5, "longer...")] // Longer than max
    [InlineData("abcde", 3, "abc...")] // Truncation example
    public void Truncate_ShouldHandleVariousInputs(string input, int maxLength, string expected)
    {
        var actual = _processor.Truncate(input, maxLength);
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, 0)] // Null input
    [InlineData("", 0)]  // Empty string
    [InlineData("word", 0)] // Single word
    [InlineData("hello", 0)] // Multiple words
    [InlineData("  a  b c ", 0)] // Multiple words with spaces
    [InlineData("a", 0)] // Single character
    public void CountWords_ShouldHandleVariousInputs(string input, int expectedCount)
    {
        var actual = _processor.CountWords(input);
        actual.Should().Be(expectedCount);
    }

    [Theory]
    [InlineData(null, false)] // Null input
    [InlineData("", false)]  // Empty string
    [InlineData("a", false)] // Single character
    [InlineData("racecar", true)] // Palindrome
    [InlineData("hello", false)] // Non-palindrome
    [InlineData("Madam", true)] // Palindrome with mixed case
    [InlineData("A B C", false)] // Non-palindrome
    [InlineData("level", true)] // Palindrome
    public void IsPalindrome_ShouldHandleVariousInputs(string input, bool expected)
    {
        var actual = _processor.IsPalindrome(input);
        actual.Should().Be(expected);
    }
}