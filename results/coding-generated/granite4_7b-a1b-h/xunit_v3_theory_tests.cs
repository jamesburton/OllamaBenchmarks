using Xunit;
using AwesomeAssertions;

public class StringProcessorTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("short")]
    [InlineData("exactlymaxlength")]
    [InlineData("toolongforthistestbutitshouldstillwork")]
    public void Truncate_ShouldHandleVariousInputs(string input)
    {
        var result = StringProcessor.Truncate(input, 10);

        switch (input)
        {
            case null:
                result.Should().Be("");
                break;
            case "":
                result.Should().Be("");
                break;
            case "short":
                result.Should().Be("short");
                break;
            case "exactlymaxlength":
                result.Should().Be("exactlymax...");
                break;
            case "toolongforthistestbutitshouldstillwork":
                result.Should().Be("toolongfor...");
                break;
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("single")]
    [InlineData("multiple words")]
    [InlineData("extra   spaces")]
    public void CountWords_ShouldHandleVariousInputs(string input)
    {
        var result = StringProcessor.CountWords(input);

        switch (input)
        {
            case null:
                result.Should().Be(0);
                break;
            case "":
                result.Should().Be(0);
                break;
            case "single":
                result.Should().Be(1);
                break;
            case "multiple words":
                result.Should().Be(2);
                break;
            case "extra   spaces":
                result.Should().Be(3);
                break;
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("racecar")]
    [InlineData("hello")]
    [InlineData("A man a plan a canal Panama")]
    public void IsPalindrome_ShouldHandleVariousInputs(string input)
    {
        var result = StringProcessor.IsPalindrome(input);

        switch (input)
        {
            case null:
                result.Should().BeFalse();
                break;
            case "":
                result.Should().BeFalse();
                break;
            case "a":
                result.Should().BeTrue();
                break;
            case "racecar":
                result.Should().BeTrue();
                break;
            case "hello":
                result.Should().BeFalse();
                break;
            case "A man a plan a canal Panama":
                result.Should().BeTrue();
                break;
        }
    }
}