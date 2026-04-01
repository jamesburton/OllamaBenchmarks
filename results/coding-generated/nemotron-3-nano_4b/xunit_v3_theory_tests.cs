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

static class StringProcessorTests
{
    // Truncate tests
    [Fact]
    public void Truncate_Scenarios(string input, int maxLength)
    {
        var processor = new StringProcessor();

        switch (input)
        {
            case null: processor.Truncate(input, maxLength).Should().Be(""); break;
            case string.Empty: processor.Truncate(input, maxLength).Should().Be(string.Empty); break;
            default:
                processor.Truncate(input, maxLength).Should().Be(input);
                if (input.Length > maxLength)
                {
                    string result = input[..maxLength] + "...";
                    processor.Truncate(input, maxLength).Should().Be(result);
                }
        }
    }

    // CountWords tests
    [Fact]
    public void CountWords_Scenarios(string input)
    {
        var processor = new StringProcessor();

        if (input is null || string.IsNullOrWhiteSpace(input))
        {
            processor.CountWords(input).Should().Be(0);
        }
        else if (input.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 1)
        {
            processor.CountWords(input).Should().Be(1);
        }
        else
        {
            processor.CountWords(input).Should().Be(input.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length);
        }
    }

    // IsPalindrome tests
    [Fact]
    public void IsPalindrome_Scenarios(string input)
    {
        var processor = new StringProcessor();

        if (string.IsNullOrEmpty(input))
        {
            processor.IsPalindrome(input).Should().Be(false);
        }
        else if (input.Length == 1)
        {
            processor.IsPalindrome(input).Should().Be(true);
        }
        else
        {
            var cleaned = input.ToLowerInvariant().Replace(" ", "");
            processor.IsPalindrome(input).Should().Be(cleaned.SequenceEqual(cleaned.Reverse()));
        }
    }
}

class StringData
{
    public static object[] Data =>
        new[]
        {
            null,
            string.Empty,
            "hi",
            "abc",
            "hello"
        };
}

class TruncateData
{
    public static object[] Data =>
        new[]
        {
            new[] { null, 5 },
            new[] { string.Empty, 3 },
            new[] { "hi", 3 },
            new[] { "abc", 3 },
            new[] { "hello", 3 }
        };
}

class CountWordsData
{
    public static object[] Data =>
        new[]
        {
            null,
            string.Empty,
            "hello",
            "hello world",
            "   a   b"
        };
}