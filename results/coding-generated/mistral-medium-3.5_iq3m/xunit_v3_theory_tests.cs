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
    public class Truncate
    {
        [Theory]
        [InlineData(null, 5, "")]
        [InlineData("", 5, "")]
        [InlineData("abc", 5, "abc")]
        [InlineData("abcde", 5, "abcde")]
        [InlineData("abcdefgh", 5, "abcde...")]
        public void ReturnsExpectedResult(string input, int maxLength, string expected)
        {
            var processor = new StringProcessor();
            var result = processor.Truncate(input, maxLength);
            result.Should().Be(expected);
        }
    }

    public class CountWords
    {
        [Theory]
        [InlineData(null, 0)]
        [InlineData("", 0)]
        [InlineData("hello", 1)]
        [InlineData("hello world", 2)]
        [InlineData("  hello   world  ", 2)]
        public void ReturnsExpectedResult(string input, int expected)
        {
            var processor = new StringProcessor();
            var result = processor.CountWords(input);
            result.Should().Be(expected);
        }
    }

    public class IsPalindrome
    {
        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("a", true)]
        [InlineData("racecar", true)]
        [InlineData("hello", false)]
        [InlineData("A man a plan a canal Panama", true)]
        public void ReturnsExpectedResult(string input, bool expected)
        {
            var processor = new StringProcessor();
            var result = processor.IsPalindrome(input);
            result.Should().Be(expected);
        }
    }
}