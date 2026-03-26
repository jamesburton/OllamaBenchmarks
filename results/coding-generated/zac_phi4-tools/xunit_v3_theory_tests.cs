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
    [InlineData(null, 5)]
    [InlineData("", 5)]
    [InlineData("test", 10)]
    [InlineData("exact", 5)]
    [InlineData("exceeds", 3)]
    public void Truncate_ShouldHandleVariousInputs(string input, int maxLength)
    {
        var result = _processor.Truncate(input, maxLength);

        if (input is null) 
            result.Should().Be("");
        else if (input.Length <= maxLength) 
            result.Should().Be(input);
        else 
            result.Should().Be(input[..maxLength] + "...");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("word")]
    [InlineData("multiple words here")]
    [InlineData("   extra spaces  ")]
    public void CountWords_ShouldCountCorrectly(string input)
    {
        var result = _processor.CountWords(input);

        if (string.IsNullOrWhiteSpace(input)) 
            result.Should().Be(0);
        else 
            result.Should().Be(input.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("racecar")]
    [InlineData("hello")]
    [InlineData("A man a plan a canal Panama")]
    public void IsPalindrome_ShouldDetectPalindromes(string input)
    {
        var result = _processor.IsPalindrome(input);

        if (string.IsNullOrEmpty(input)) 
            result.Should().BeFalse();
        else
        {
            var cleaned = input.ToLowerInvariant().Replace(" ", "");
            result.Should().Be(cleaned.SequenceEqual(cleaned.Reverse()));
        }
    }
}