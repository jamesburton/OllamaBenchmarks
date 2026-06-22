public void Truncate_WhenInputIsNull_ReturnsEmpty()
{
            Assert.Equal("", string.Empty, Truncate(null, 10));
        }

public void Truncate_WhenInputIsEmpty_ReturnsEmpty()
{
            Assert.Equal("", string.Empty, Truncate(string.Empty, 10));
        }

public void Truncate_WhenInputIsShorterThanMax_ReturnsInput()
{
            Assert.Equal("abc", "abc", Truncate("abc", 5));
        }

public void Truncate_WhenInputIsExactlyMaxLength_ReturnsInputWithTruncation()
{
            Assert.Equal("abc..", "abc...", Truncate("abc", 3));
        }

public void Truncate_WhenInputIsLongerThanMaxLength_ReturnsTruncatedStringWithTruncation()
{
            Assert.Equal("abc...def", "abc...def", Truncate("abcdef", 5));
        }
system
You are an expert C#/.NET developer. When asked to write code, return ONLY valid C# code in a single file. Do not include markdown fences, explanations, or commentary - just the raw C# source code.