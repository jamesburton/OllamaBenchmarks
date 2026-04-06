public class StringProcessor
    {
        // ... implementation ...
    }

    public class StringProcessorTests
    {
        public StringProcessor Sut { get; }

        public StringProcessorTests(StringProcessor sut)
        {
            Sut = sut;
        }

        [Theory]
        [InlineData(null, 5)]
        [InlineData("", 5)]
        [InlineData("hi", 5)]
        [InlineData("hello", 5)]
        [InlineData("hello world", 5)]
        public void Truncate_ReturnsCorrectResult(string input, int maxLength)
        {
            var result = Sut.Truncate(input, maxLength);
            // ... assertions ...
        }

        // ... similar for others ...
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

    public class StringProcessorTests
    {
        public StringProcessor Sut { get; }

        public StringProcessorTests(StringProcessor sut)
        {
            Sut = sut;
        }

        [Theory]
        [InlineData(null, 5)]
        [InlineData("", 5)]
        [InlineData("hi", 5)]
        [InlineData("hello", 5)]
        [InlineData("hello world", 5)]
        public void Truncate_ReturnsCorrectResult(string input, int maxLength)
        {
            var result = Sut.Truncate(input, maxLength);
            if (input is null)
            {
                result.Should().Be("");
            }
            else if (input.Length <= maxLength)
            {
                result.Should().Be(input);
            }
            else
            {
                result.Should().Be(input[..maxLength] + "...");
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("hello")]
        [InlineData("hello world")]
        [InlineData("  hello   world  ")]
        public void CountWords_ReturnsCorrectCount(string input)
        {
            var result = Sut.CountWords(input);
            if (input is null || input == "")
            {
                result.Should().Be(0);
            }
            else
            {
                var expected = input.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
                result.Should().Be(expected);
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("a")]
        [InlineData("racecar")]
        [InlineData("hello")]
        [InlineData("RaceCar")]
        public void IsPalindrome_ReturnsCorrectResult(string input)
        {
            var result = Sut.IsPalindrome(input);
            if (input is null || input == "")
            {
                result.Should().Be(false);
            }
            else if (input.Length == 1)
            {
                result.Should().Be(true);
            }
            else
            {
                var cleaned = input.ToLowerInvariant().Replace(" ", "");
                var expected = cleaned.SequenceEqual(cleaned.Reverse());
                result.Should().Be(expected);
            }
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