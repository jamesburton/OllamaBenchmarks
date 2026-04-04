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

[TestFixture]
class MyTests
{
    [TestMethod]
    [Theory]
    [InlineData(null)]
    public void Truncate_Should_Return_EmptyString_When_InputIsNull(int maxLength)
    {
        var sp = new StringProcessor();
        var actual = sp.Truncate(null, maxLength);
        actual.Should().Be("");

        var sp2 = new StringProcessor();
        var actual2 = sp2.Truncate(string.Empty, maxLength);
        actual2.Should().Be(string.Empty);
    }

    [TestMethod]
    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(10)]
    public void Truncate_Should_Return_TruncatedString_When_InputIsShortOrEqualToMaxLength(int maxLength)
    {
        var sp = new StringProcessor();
        var testStrings = new[] { "", "a", "ab" };
        foreach (var s in testStrings)
        {
            var expected = s;
            var actual = sp.Truncate(s, maxLength);
            actual.Should().Be(expected);
        }
    }

    [TestMethod]
    [Theory]
    [InlineData(2)]
    public void Truncate_Should_Return_TruncatedString_When_InputIsLongerThanMaxLength(int maxLength)
    {
        var sp = new StringProcessor();
        var actual = sp.Truncate("123456789", maxLength);
        actual.Should().Be("123...");
    }

    [TestMethod]
    [Theory]
    public void CountWords_Should_Return_Zero_When_InputIsNull_or_WhiteSpace(int nullCheck)
    {
        var sp = new StringProcessor();
        var nullActual = sp.CountWords(null);
        nullActual.Should().Be(0);

        var spaceActual = sp.CountWords("   ");
        spaceActual.Should().Be(0);
    }

    [TestMethod]
    [Theory]
    [InlineData("single")]
    [InlineData("multiple words")]
    [InlineData("   a   b   ")]
    public void CountWords_Should_Return_CorrectCount(string input)
    {
        var sp = new StringProcessor();
        var actual = sp.CountWords(input);
        actual.Should().Be(input.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length);
    }

    [TestMethod]
    [Theory]
    [InlineData(null)]
    public void IsPalindrome_Should_Return_False_When_InputIsNull(int nullCheck)
    {
        var sp = new StringProcessor();
        var actual = sp.IsPalindrome(null);
        actual.Should().Be(false);
    }

    [TestMethod]
    [Theory]
    public void IsPalindrome_Should_Return_False_When_InputIsEmptyOrSingleChar(input)
    {
        var sp = new StringProcessor();
        var result = sp.IsPalindrome("");
        result.Should().Be(false);

        result = sp.IsPalindrome("a");
        result.Should().Be(true);
    }

    [TestMethod]
    [Theory]
    [InlineData("racecar")]
    [InlineData("RaceCar")]
    public void IsPalindrome_Should_Return_True_When_InputIsACaseInsensitivePalindrome(string input)
    {
        var sp = new StringProcessor();
        var actual = sp.IsPalindrome(input);
        actual.Should().Be(true);
    }

    [TestMethod]
    [Theory]
    [InlineData("hello")]
    public void IsPalindrome_Should_Return_False_When_InputIsNonPalindrome(string input)
    {
        var sp = new StringProcessor();
        var actual = sp.IsPalindrome(input);
        actual.Should().Be(false);
    }
}