public class StringProcessor { ... }

        public class StringProcessorTests
        {
            private readonly StringProcessor _sut = new();

            [Theory]
            [InlineData(null, 5, "")]
            [InlineData("", 5, "")]
            [InlineData("abc", 5, "abc")]
            [InlineData("abc", 3, "abc")]
            [Inline ... ]
            public void Truncate_Tests(string input, int maxLength, string expected)
            {
                var result = _sut.Truncate(input, maxLength);
                result.Should().Be(expected);
            }

            [Theory]
            [InlineData(null, 0)]
            [InlineData("", 0)]
            [InlineData("word", 1)]
            [InlineData("two words", 2)]
            [InlineData("  extra  spaces  ", 2)]
            public void CountWords_Tests(string input, int expected)
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
            public void IsPalindrome_Tests(string input, bool expected)
            {
                var result = _sut.IsPalindrome(input);
                result.Should().Be(expected);
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

    public class StringProcessorTests
    {
        private readonly StringProcessor _sut = new();

        [Theory]
        [InlineData(null, 5, "")]
        [InlineData("", 5, "")]
        [InlineData("short", 10, "short")]
        [InlineData("exact", 5, "exact")]
        [InlineData("longerthanmax", 5, "longe...")] // Wait, logic is input[..maxLength] + "..."
        // If maxLength is 5, input[..5] is "longe". Result "longe..."
        // Let's re-verify: input[..5] is indices 0,1,2,3,4.
        // "longerthanmax"[..5] -> "longe"
        // "longe" + "..." -> "longe..."
        // Let's use a simpler one: "abcdef", 3 -> "abc..."
        [InlineData("abcdef", 3, "abc...")]
        public void Truncate_ShouldReturnExpectedResult(string input, int maxLength, string expected)
        {
            var result = _sut.Truncate(input, maxLength);
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(null, 0)]
        [InlineData("", 0)]
        [InlineData("single", 1)]
        [InlineData("two words", 2)]
        [InlineData("  extra  spaces  ", 2)]
        public void CountWords_ShouldReturnCorrectCount(string input, int expected)
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
        public void IsPalindrome_ShouldReturnCorrectBoolean(string input, bool expected)
        {
            var result = _sut.IsPalindrome(input);
            result.Should().Be(expected);
        }
    }