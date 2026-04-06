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
    [Fact]
    public void Truncate_ShouldTruncateStringAsExpected(string input, int maxLength)
    {
        var p = new StringProcessor();
        var result = p.Truncate(input, maxLength);

        if (maxLength >= result.Length) result.Should().Be(input);
        else result.Should().Be(input [..] maxLength + "...");
    }

    [Theory]
    [InlineData(null, 0)]
    [InlineData("", 0)]
    [InlineData("short", 5)]
    [InlineData("short===", 5)]
    [InlineData("long=============", 5)]
    public void Truncate_ShouldTruncateStringAsExpected_Parameterized(string input, int maxLength)
    {
        var p = new StringProcessor();
        var result = p.Truncate(input, maxLength);
        if (maxLength >= result.Length) result.Should().Be(input);
        else result.Should().Be(input [..] maxLength + "...");
    }

    [Fact]
    public void CountWords_ShouldReturnZeroForNullOrWhiteSpace(string input)
    {
        var p = new StringProcessor();
        var result = p.CountWords(input);
        result.Should().Be(0);
    }

    [Fact]
    public void CountWords_ShouldCountWords(string input)
    {
        var p = new StringProcessor();
        var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var expected = words.Length;
        var result = p.CountWords(input);
        result.Should().Be(expected);
    }

    [Fact]
    public void IsPalindrome_ShouldBeTrueForPalindromes(string input)
    {
        var p = new StringProcessor();
        var result = p.IsPalindrome(input);
        result.Should().BeTrue();
    }

    [Fact]
    public void IsPalindrome_ShouldBeFalseForNonPalindromes(string input)
    {
        var p = new StringProcessor();
        var result = p.IsPalindrome(input);
        result.Should().BeFalse();
    }

    [Fact]
    public void IsPalindrome_ShouldHandleNullAndEmpty(string input)
    {
        var p = new StringProcessor();
        var result = p.IsPalindrome(input);
        result.Should().BeFalse();
    }

    [Fact]
    public void IsPalindrome_ShouldReturnTrueForSingleChar(string input)
    {
        var p = new StringProcessor();
        var result = p.IsPalindrome(input);
        result.Should().BeTrue();
    }

    [Fact]
    public void IsPalindrome_ShouldReturnTrueForMixedCasePalindrome(string input)
    {
        var p = new StringProcessor();
        var result = p.IsPalindrome(input);
        result.Should().BeTrue();
    }
}