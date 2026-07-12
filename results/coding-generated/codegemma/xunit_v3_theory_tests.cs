public class StringProcessorTests
{
    private readonly StringProcessor _processor = new StringProcessor();

    [Theory]
    [InlineData(null, 0)]
    [InlineData("", 0)]
    [InlineData("hello", 5)]
    [InlineData("world", 5)]
    [InlineData("This is a string", 14)]
    public void CountWords_ReturnsCorrectCount(string input, int expected)
    {
        int actual = _processor.CountWords(input);
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("hello", "hello")]
    [InlineData("hello world", "hello world")]
    [InlineData("This is a string", "This is a string")]
    [InlineData("This is a string longer than max", "This is a string lon...")]
    public void Truncate_ReturnsTruncatedString(string input, string expected)
    {
        string actual = _processor.Truncate(input, 14);
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("a", true)]
    [InlineData("racecar", true)]
    [InlineData("madam", true)]
    [InlineData("hello", false)]
    public void IsPalindrome_ReturnsCorrectValue(string input, bool expected)
    {
        bool actual = _processor.IsPalindrome(input);
        actual.Should().Be(expected);
    }
}