public class StringProcessor
{
    public string Truncate(string input, int maxLength)
    {
        if (input == null) return "";
        return input.Length <= maxLength ? input : input[..maxLength] + "....";
    }

    [Theory] [InlineData(0)]
    [InlineData(0