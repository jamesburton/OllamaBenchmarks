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

[Theory]
[InlineData(null, 5)]
[InlineData("", 5)]
[InlineData("abc", 5)]
[InlineData("abcde", 5)]
[InlineData("abcdef", 5)]
public void TruncateTest(string input, int maxLength)
{
    var processor = new StringProcessor();
    var result = processor.Truncate(input, maxLength);
    var expected = input is null ? "" : 
                   input.Length <= maxLength ? input : 
                   input[..maxLength] + "...";
    result.Should().Be(expected);
}

[Theory]
[InlineData(null)]
[InlineData("")]
[InlineData("hello")]
[InlineData("hello world")]
[InlineData("   hello   ")]
public void CountWordsTest(string input)
{
    var processor = new StringProcessor();
    var result = processor.CountWords(input);
    var expected = input is null || string.IsNullOrWhiteSpace(input) ? 0 : input.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
    result.Should().Be(expected);
}

[Theory]
[InlineData(null)]
[InlineData("")]
[InlineData("a")]
[InlineData("madam")]
[InlineData("hello")]
[InlineData("RaceCar")]
public void IsPalindromeTest(string input)
{
    var processor = new StringProcessor();
    var result = processor.IsPalindrome(input);
    var expected = input is null || string.IsNullOrEmpty(input) ? false : 
                   input.ToLowerInvariant().Replace(" ", "").SequenceEqual(input.ToLowerInvariant().Replace(" ", "").Reverse());
    result.Should().Be(expected);
}