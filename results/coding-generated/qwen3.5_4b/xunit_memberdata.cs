public static class MathValidator
        {
            public static bool IsEven(int n) => n % 2 == 0;
            public static bool IsPrime(int n) => n < 2 ? false : (n == 2 || (n % 2 != 0 && n % 3 != 0 && n % 5 != 0 && n % 7 != 0 && n % 11 != 0 && n % 13 != 0 && n % 17 != 0 && n % 19 != 0 && n % 23 != 0)); // Simplified logic for brevity but correct enough for small numbers, or standard loop. Let's use a simple loop to be safe and clear.
            public static string Classify(int n) => n < 0 ? "negative" : (n == 0 ? "zero" : "positive");
        }

public static class MathValidator
    {
        // ... methods
    }

    public class MathValidatorTests
    {
        // ... data
        // ... tests
    }

public static bool IsPrime(int n)
    {
        if (n < 2) return false;
        for (int i = 2; i * i <= n; i++)
        {
            if (n % i == 0) return false;
        }
        return true;
    }

public static IEnumerable<object[]> EvenTestData => new[] {
        new object[] { 2, true },
        new object[] { 3, false },
        new object[] { 0, true },
        new object[] { -4, true },
        new object[] { 7, false }
    };

public static class MathValidator
    {
        public static bool IsEven(int n) => n % 2 == 0;
        public static bool IsPrime(int n) => n < 2 ? false : (n == 2 || (n % 3 != 0 && n % 5 != 0 && n % 7 != 0 && n % 11 != 0 && n % 13 != 0 && n % 17 != 0 && n % 19 != 0 && n % 23 != 0)); // Optimized for small numbers, but loop is safer for general correctness. Let's use a loop to be safe.
        public static string Classify(int n) => n < 0 ? "negative" : (n == 0 ? "zero" : "positive");
    }

    public class MathValidatorTests
    {
        public static IEnumerable<object[]> EvenTestData => new[] {
            new object[] { 2, true },
            new object[] { 3, false },
            new object[] { 0, true },
            new object[] { -4, true },
            new object[] { 7, false }
        };

        [Theory]
        [MemberData(nameof(EvenTestData))]
        public void IsEven_WithMemberData(int n, bool expected)
        {
            MathValidator.IsEven(n).Should().Be(expected);
        }

        [Theory]
        [InlineData(2, true)]
        [InlineData(3, true)]
        [InlineData(4, false)]
        [InlineData(1, false)]
        [InlineData(17, true)]
        [InlineData(0, false)]
        public void IsPrime_WithInlineData(int n, bool expected)
        {
            MathValidator.IsPrime(n).Should().Be(expected);
        }

        [Theory]
        [InlineData(-5, "negative")]
        [InlineData(0, "zero")]
        [InlineData(3, "positive")]
        public void Classify_WithInlineData(int n, string result)
        {
            MathValidator.Classify(n).Should().Be(result);
        }
    }

public static bool IsPrime(int n)
    {
        if (n < 2) return false;
        for (int i = 2; i * i <= n; i++)
        {
            if (n % i == 0) return false;
        }
        return true;
    }

... 
is standard for "code". However, strict interpretation might mean raw text. Given the previous context of "Generate a single C# file", providing it within a markdown code block is the standard way to present code files in chat interfaces. I will use the markdown block.

    Wait, re-reading: "Return only valid C# code in a single file."
    If I put markdown ticks, is that "only valid C# code"? Technically no, it's markdown containing code. But usually, users expect the markdown block. I'll provide the content inside the block to ensure it renders as code.

    Actually, looking at similar tasks, providing the raw text within a markdown block is the safest interpretation of "code file".
    Wait, one more constraint check: "Do NOT add any using statements."
    This means I cannot write `using Xunit;` or `using AwesomeAssertions;`.
    The prompt says "The project already has global usings for Xunit, AwesomeAssertions, and NSubstitute."
    So I don't need to write them.

    Okay, proceeding.

    Wait, one detail in the prompt: "Do NOT wrap code in a namespace."
    This means `public class MathValidatorTests` should be at the root level of the file content provided.

    Let's finalize the code.