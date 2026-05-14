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
[Fact]
public void Truncate_Methods_Parameters_NoUnitTests_Truncate_Should_BeNull_Method_Should_Not_Return_But_Empty_String()
{
    var processor = new StringProcessor();
    var result = processor.Truncate(null, 10);
    result.Should().Be("");
}
[Theory]
public void Truncate_Methods_Parameters_Truncate_Should_BeNull_Method_Should_Not_Return_But_Empty_String(
    [InLineData(null)] int dummy
)
{
    var processor = new StringProcessor();
    var result = processor.Truncate(null, 10);
    result.Should().Be("");
}
[InlineData("")]
[InLineData("a")]
[InLineData("ab")]
[InLineData("abc")]


[InLineData(0)]


[InLineData(5)]
[InLineData(10)]
[InLineData(2)]


[InLineData(5)]
[InLineData(6)]
[InLineData(7)]
[InLineData(8)]
[InLineData(9)]
[InLineData(3)]
[InLineData(5)]


[InLineData(4)]


[InLineData(0)]
[InLineData(0)]
[InLineData(10)]
[InLineData(13)
[InLineData(10)]
[InLineData(122)]


[InLineData(10)
[InLineData(2)]
[InLineData(0)
[InLineData(10)]
[InLineData(122)