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
        [Theory]
        [InlineData(null, 5)]
        [InlineData("", 5)]
        [InlineData("hello", 3)]
        [InlineData("hello", 5)]
        [InlineData("hello", 2)]
        public void TestTruncate(string input, int maxLength)
        {
            var sut = new StringProcessor();
            var result = sut.Truncate(input, maxLength);

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
                result.Should().Be(input[..maxLength] + "..");
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("hello")]
        [InlineData("hello world")]
        [InlineData("  hello  world  ")]
        public void TestCountWords(string input)
        {
            var sut = new StringProcessor();
            var result = sut.CountWords(input);

            if (input is null)
            {
                result.Should().Be(0);
            }
            else if (string.IsNullOrWhiteSpace(input))
            {
                result.Should().Be(0);
            }
            else
            {
                result.Should().Be(input.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length);
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("a")]
        [InlineData("racecar")]
        [InlineData("hello")]
        [InlineData("RaceCar")]
        public void TestIsPalindrome(string input)
        {
            var sut = new StringProcessor();
            var result = sut.IsPalindrome(input);

            if (input is null)
            {
                result.Should().Be(false);
            }
            else if (string.IsNullOrEmpty(input))
            {
                result.Should().Be(false);
            }
            else if (input.Length == 1)
            {
                result.Should().Be(true);
            }
            else if (input == "racecar" || input == "RaceCar")
            {
                result.Should().Be(true);
            }
            else
            {
                result.Should().Be(false);
            }
        }
    }