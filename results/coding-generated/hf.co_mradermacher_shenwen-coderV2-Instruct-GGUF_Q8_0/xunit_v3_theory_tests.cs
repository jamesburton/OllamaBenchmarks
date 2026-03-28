using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace AwesomeAssertions
{
    public class StringProcessorTests
    {
        [Fact]
        public void Truncate_null_input()
        {
            var processor = new StringProcessor();
            string result = processor.Truncate(null, 10);
            Assert.Equal("", result);
        }

        [Fact]
        public void Truncate_empty_string()
        {
            var processor = new StringProcessor();
            string result = processor.Truncate("", 10);
            Assert.Equal("", result);
        }

        [Theory]
        public void Truncate_string_shorter_than_max()
        {
            var processor = new StringProcessor();
            string result = processor.Truncate("hello", 5);
            Assert.Equal("hello", result);
        }

        [Theory]
        public void Truncate_string_equal_to_max()
        {
            var processor = new StringProcessor();
            string result = processor.Truncate("hello", 10);
            Assert.Equal("hello", result);
        }

        [Theory]
        public void Truncate_string_longer_than_max()
        {
            var processor = new StringProcessor();
            string result = processor.Truncate("hello world", 15);
            Assert.Equal("hello world", result);
        }

        [Fact]
        public void CountWords_null_input()
        {
            var processor = new StringProcessor();
            int count = processor.CountWords(null);
            Assert.Equal(0, count);
        }

        [Fact]
        public void CountWords_empty_string()
        {
            var processor = new StringProcessor();
            int count = processor.CountWords("");
            Assert.Equal(0, count);
        }

        [Theory]
        public void CountWords_single_word()
        {
            var processor = new StringProcessor();
            int count = processor.CountWords("hello");
            Assert.Equal(1, count);
        }

        [Theory]
        public void CountWords_multiple_words()
        {
            var processor = new StringProcessor();
            string input = "this is a test";
            int count = processor.CountWords(input);
            Assert.Equal(5, count);
        }

        [Fact]
        public void CountWords_extra_spaces()
        {
            var processor = new StringProcessor();
            string input = " this  is   a test ";
            int count = processor.CountWords(input);
            Assert.Equal(7, count);
        }

        [Fact]
        public void IsPalindrome_null_input()
        {
            var processor = new StringProcessor();
            bool result = processor.IsPalindrome(null);
            Assert.True(result);
        }

        [Fact]
        public void IsPalindrome_empty_string()
        {
            var processor = new StringProcessor();
            bool result = processor.IsPalindrome("");
            Assert.True(result);
        }

        [Theory]
        public void IsPalindrome_single_character()
        {
            var processor = new StringProcessor();
            bool result = processor.IsPalindrome("a");
            Assert.True(result);
        }

        [Theory]
        public void IsPalindrome_palindrome()
        {
            var processor = new StringProcessor();
            bool result = processor.IsPalindrome("racecar");
            Assert.True(result);
        }

        [Theory]
        public void IsPalindrome_non_palindrome()
        {
            var processor = new StringProcessor();
            bool result = processor.IsPalindrome("hello world");
            Assert.False(result);
        }
    }
}