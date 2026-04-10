using Xunit;
using AwesomeAssertions;

public class StringProcessorTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("short")]
    [InlineData("exactlymaxlength")]
    [InlineData("toolongtoolongtoolong")]
    public void Truncate_ShouldHandleAllCases(string input)
    {
        var processor = new StringProcessor();
        var result = processor.Truncate(input, 10);

        switch (input)
        {
            case null:
            case "":
                result.Should().Be("");
                break;
            case var s when s.Length <= 10:
                result.Should().Be(s);
                break;
            case var s when s.Length == 10:
                result.Should().Be(s);
                break;
            case var s when s.Length > 10:
                result.Should().Be(s.Substring(0, 10) + "...");
                break;
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("single")]
    [InlineData("multiple words")]
    [InlineData("  extra   spaces  ")]
    public void CountWords_ShouldHandleAllCases(string input)
    {
        var processor = new StringProcessor();
        var result = processor.CountWords(input);

        switch (input)
        {
            case null:
                result.Should().Be(0);
                break;
            case "":
                result.Should().Be(0);
                break;
            case var s when s.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 1:
                result.Should().Be(1);
                break;
            case var s when s.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length > 1:
                result.Should().Be(s.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length);
                break;
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("racecar")]
    [InlineData("hello")]
    [InlineData("Madam")]
    public void IsPalindrome_ShouldHandleAllCases(string input)
    {
        var processor = new StringProcessor();
        var result = processor.IsPalindrome(input);

        switch (input)
        {
            case null:
                result.Should().BeFalse();
                break;
            case "":
                result.Should().BeFalse();
                break;
            case var s when s.Length == 1:
                result.Should().BeTrue();
                break;
            case var s when s.Equals(s.Reverse().Aggregate("", (current, c) => c + current), StringComparison.OrdinalIgnoreCase):
                result.Should().BeTrue();
                break;
            default:
                result.Should().BeFalse();
                break;
        }
    }
}