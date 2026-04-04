public class StringProcessor
{
    // ... implementation provided in prompt ...
}

public class StringProcessorTests
{
    private readonly StringProcessor _sut = new(); // System Under Test

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
    [InlineData(null, 10, "")]
    [InlineData("", 10, "")]
    [InlineData("abc", 5, "abc")]
    [InlineData("abc", 3, "abc")]
    [InlineData("abcde", 3, "abc...")]
    public void Truncate_ShouldReturnExpectedResult(string input, int maxLength, string expected)
    {
        var result = _processor.Truncate(input, maxLength);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, 0)]
    [InlineData("", 0)]
    [InlineData("   ", 0)]
    [InlineData("hello", 1)]
    [InlineData("hello world", 2)]
    [InlineData("  hello   world  ", 2)]
    public void CountWords_ShouldReturnExpectedCount(string input, int expected)
    {
        var result = _processor.CountWords(input);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("a", true)]
    [InlineData("racecar", true)]
    [InlineData("hello", false)]
    [InlineData("RaceCar", true)]
    [InlineData("A man a plan a canal Panama", true)] // Spaces are removed in logic
    public void IsPalindrome_ShouldReturnExpectedResult(string input, bool expected)
    {
        var result = _processor.IsPalindrome(input);

        result.Should().Be(expected);
    }
}

public bool IsPalindrome(string input)
    {
        if (string.IsNullOrEmpty(input)) return false;
        var cleaned = input.ToLowerInvariant().Replace(" ", "");
        return cleaned.SequenceEqual(cleaned.Reverse());
    }

public class StringProcessor
{
    // ... copy paste ...
}

public class StringProcessorTests
{
    private readonly StringProcessor _sut = new StringProcessor();

    [Theory]
    [InlineData(null, 10, "")]
    [InlineData("", 10, "")]
    [InlineData("abc", 5, "abc")]
    [InlineData("abc", 3, "abc")]
    [InlineData("abcde", 3, "abc...")]
    public void Truncate_ShouldHandleVariousInputs(string input, int maxLength, string expected)
    {
        var result = _sut.Truncate(input, maxLength);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, 0)]
    [InlineData("", 0)]
    [InlineData("hello", 1)]
    [InlineData("hello world", 2)]
    [InlineData("  hello   world  ", 2)]
    public void CountWords_ShouldHandleVariousInputs(string input, int expected)
    {
        var result = _sut.CountWords(input);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("a", true)]
    [InlineData("racecar", true)]
    [InlineData("hello", false)]
    [InlineData("RaceCar", true)]
    public void IsPalindrome_ShouldHandleVariousInputs(string input, bool expected)
    {
        var result = _sut.IsPalindrome(input);
        result.Should().Be(expected);
    }
}

[Fact]
public void Truncate_ShouldReturnEmptyString_WhenInputIsNull()
{
    var result = _sut.Truncate(null, 10);
    result.Should().Be("");
}

[Theory]
[InlineData(null)]
public void Truncate_ShouldReturnEmpty_WhenInputIsNull(string input) { ... }