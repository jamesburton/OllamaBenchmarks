using Xunit;
using AwesomeAssertions;

namespace StringProcessorTests
{
    public class StringProcessorTests
    {
        [Fact]
        public void Truncate_NullInput()
        {
            string result = new StringProcessor().Truncate(null, 10);
            Assert.Equal("", result);
        }

        [Fact]
        public void Truncate_EmptyString()
        {
            string result = new StringProcessor().Truncate("", 10);
            Assert.Equal("", result);
        }

        [Fact]
        public void Truncate_ShortThanMax()
        {
            string input = "hello";
            int maxLength = 5;
            string expectedResult = "hello";
            string result = new StringProcessor().Truncate(input, maxLength);

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void Truncate_EqualToMax()
        {
            string input = "hello world";
            int maxLength = 10;
            string expectedResult = "hello world";
            string result = new StringProcessor().Truncate(input, maxLength);

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void Truncate_LongThanMax()
        {
            string input = "hello world and more stuff";
            int maxLength = 10;
            string expectedResult = "hello world ...";
            string result = new StringProcessor().Truncate(input, maxLength);

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void CountWords_NullInput()
        {
            int count = new StringProcessor().CountWords(null);
            Assert.Equal(0, count);
        }

        [Fact]
        public void CountWords_EmptyString()
        {
            int count = new StringProcessor().CountWords("");
            Assert.Equal(0, count);
        }

        [Fact]
        public void CountWords_SingleWord()
        {
            string input = "hello";
            int expectedCount = 1;
            int result = new StringProcessor().CountWords(input);

            Assert.Equal(expectedCount, result);
        }

        [Fact]
        public void CountWords_MultipleWords()
        {
            string input = "hello world";
            int expectedCount = 2;
            int result = new StringProcessor().CountWords(input);

            Assert.Equal(expectedCount, result);
        }

        [Fact]
        public void CountWords_ExtraSpaces()
        {
            string input = "   hello    world     ";
            int expectedCount = 3;
            int result = new StringProcessor().CountWords(input);

            Assert.Equal(expectedCount, result);
        }