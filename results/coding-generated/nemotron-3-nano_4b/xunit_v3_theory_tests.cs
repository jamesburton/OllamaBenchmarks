public class StringProcessorTest
{
    [Fact]
    public void Truncate_TruncatesLongerThanMax_ShouldBeTruncated(string input, int maxLength)
    {
        var result = new StringProcessor().Truncate(input, maxLength);

        // Expected result based on length comparison
        string expected = (input == null) 
            ? ""
            : (input.Length <= maxLength) ? input : input[..maxLength] + "...";

        result.Should().Be(expected);
    }

    [Theory, InlineData(null)]
    [Fact]
    public void Truncate_EmptyString_ShouldBeEmpty(string input, int maxLength)
    {
        var result = new StringProcessor().Truncate(input, maxLength);
        result.Should().Be("");
    }

    [Theory, InlineData("short")]
    [Fact]
    public void Truncate_StringShorterThanMax_ShouldBeUnchanged(string input, int maxLength)
    {
        var result = new StringProcessor().Truncate(input, maxLength);
        result.Should().Be(input);
    }

    [Theory, InlineData("equals")]
    [Fact]
    public void Truncate_StringEqualToMax_ShouldBeUnchanged(string input, int maxLength)
    {
        var result = new StringProcessor().Truncate(input, maxLength);
        result.Should().Be(input);
    }

    [Theory, InlineData("abcdefg")]
    [Fact]
    public void Truncate_StringLongerThanMax_ShouldBeTruncated(string input, int maxLength)
    {
        var result = new StringProcessor().Truncate(input, maxLength);
        result.Should().Be("abcde...");
    }

    [Theory, InlineData(null)]
    [Fact]
    public void CountWords_InputNull_ShouldReturnZero(string input, int maxLength)
    {
        var result = new StringProcessor().CountWords(input);
        result.Should().Be(0);
    }

    [Theory, InlineData("")]
    [Fact]
    public void CountWords_EmptyString_ShouldReturnZero(string input, int maxLength)
    {
        var result = new StringProcessor().CountWords(input);
        result.Should().Be(0);
    }

    [Theory, InlineData("Hello")]
    [Fact]
    public void CountWords_SingleWord_ShouldReturnOne(string input, int maxLength)
    {
        var result = new StringProcessor().CountWords(input);
        result.Should().Be(1);
    }

    [Theory, InlineData("Hello World")]
    [Fact]
    public void CountWords_MultipleWords_ShouldReturnTwo(string input, int maxLength)
    {
        var result = new StringProcessor().CountWords(input);
        result.Should().Be(2);
    }

    [Theory, InlineData("   Hello   World   ")]
    [Fact]
    public void CountWords_ExtraSpaces_ShouldReturnTwo(string input, int maxLength)
    {
        var result = new StringProcessor().CountWords(input);
        result.Should().Be(2);
    }

    [Fact]
    public void IsPalindrome_Null_ShouldReturnFalse(string input)
    {
        var result = new StringProcessor().IsPalindrome(input);
        result.Should().Be(false);
    }

    [Fact]
    public void IsPalindrome_EmptyString_ShouldReturnFalse(string input)
    {
        var result = new StringProcessor().IsPalindrome(input);
        result.Should().Be(false);
    }

    [Fact]
    public void IsPalindrome_SingleCharacter_ShouldReturnTrue(string input)
    {
        var result = new StringProcessor().IsPalindrome(input);
        result.Should().Be(true);
    }

    [Fact]
    public void IsPalindrome_Palindrome_ShouldReturnTrue(string input)
    {
        var result = new StringProcessor().IsPalindrome(input);
        result.Should().Be(true);
    }

    [Fact]
    public void IsPalindrome_NonPalindrome_ShouldReturnFalse(string input)
    {
        var result = new StringProcessor().IsPalindrome(input);
        result.Should().Be(false);
    }

    [Fact]
    public void IsPalindrome_PalindromeMixedCase_ShouldReturnTrue(string input)
    {
        var result = new StringProcessor().IsPalindrome("Aba");
        result.Should().Be(true);
    }
}

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