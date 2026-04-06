public static class MathValidator
{
    public static bool IsEven(int n) => n % 2 == 0;

    public static bool IsPrime(int n)
    {
        if (n < 2) return false;
        for (int i = 2; i <= Math.Sqrt(n); i++)
            if (n % i == 0) return false;
        return true;
    }

    public static string Classify(int n)
    {
        if (n < 0) return "negative";
        else if (n == 0) return "zero";
        else return "positive";
    }
}

public class MathValidatorTests
{
    public static IEnumerable<object[]> EvenTestData =>
        new object[][]
        {
            new object[] {2, true},
            new object[] {3, false},
            new object[] {0, true},
            new object[] {-4, true},
            new object[] {7, false}
        };

    [Theory]
    public void IsEven_WithMemberData([MemberData(nameof(EvenTestData))object[] data) 
    {
        int n = (int)data[0];
        bool expected = (bool)data[1];
        MathValidator.IsEven(n).Should().Be(expected);
    }

    [Theory]
    [InlineData(2, true)]
    public void IsPrime_WithInlineData(int n, bool expected)
    {
        MathValidator.IsPrime(n).Should().Be(expected);
    }

    [Theory]
    [InlineData(-5, "negative")]
    [InlineData(0, "zero")]
    [InlineData(3, "positive")]
    public void Classify_WithInlineData(int n, string expected)
    {
        MathValidator.Classify(n).Should().Be(expected);
    }
}

<code>
public static class MathValidator
{
    public static bool IsEven(int n) => n % 2 == 0;

    public static bool IsPrime(int n)
    {
        if (n < 2) return false;
        for (int i = 2; i <= Math.Sqrt(n); i++)
            if (n % i == 0) return false;
        return true;
    }

    public static string Classify(int n)
    {
        if (n < 0) return "negative";
        else if (n == 0) return "zero";
        else return "positive";
    }
}

public class MathValidatorTests
{
    public static IEnumerable<object[]> EvenTestData =>
        new object[][]
        {
            new object[] {2, true},
            new object[] {3, false},
            new object[] {0, true},
            new object[] {-4, true},
            new object[] {7, false}
        };

    [Theory]
    public void IsEven_WithMemberData([MemberData(nameof(EvenTestData))object[] data) 
    {
        int n = (int)data[0];
        bool expected = (bool)data[1];
        MathValidator.IsEven(n).Should().Be(expected);
    }

    [Theory]
    [InlineData(2, true)]
    public void IsPrime_WithInlineData(int n, bool expected)
    {
        MathValidator.IsPrime(n).Should().Be(expected);
    }

    [Theory]
    [InlineData(-5, "negative")]
    [InlineData(0, "zero")]
    [InlineData(3, "positive")]
    public void Classify_WithInlineData(int n, string expected)
    {
        MathValidator.Classify(n).Should().Be(expected);
    }
}

<code>
public static class MathValidator
{
    public static bool IsEven(int n) => n % 2 == 0;

    public static bool IsPrime(int n)
    {
        if (n < 2) return false;
        for (int i = 2; i <= Math.Sqrt(n); i++)
            if (n % i == 0) return false;
        return true;
    }

    public static string Classify(int n)
    {
        if (n < 0) return "negative";
        else if (n == 0) return "zero";
        else return "positive";
    }
}

public class MathValidatorTests
{
    public static IEnumerable<object[]> EvenTestData =>
        new object[][]
        {
            new object[] {2, true},
            new object[] {3, false},
            new object[] {0, true},
            new object[] {-4, true},
            new object[] {7, false}
        };

    [Theory]
    public void IsEven_WithMemberData([MemberData(nameof(EvenTestData))object[] data) 
    {
        int n = (int)data[0];
        bool expected = (bool)data[1];
        MathValidator.IsEven(n).Should().Be(expected);
    }

    [Theory]
    [InlineData(2, true)]
    public void IsPrime_WithInlineData(int n, bool expected)
    {
        MathValidator.IsPrime(n).Should().Be(expected);
    }

    [Theory]
    [InlineData(-5, "negative")]
    [InlineData(0, "zero")]
    [InlineData(3, "positive")]
    public void Classify_WithInlineData(int n, string expected)
    {
        MathValidator.Classify(n).Should().Be(expected);
    }
}

<code>
public static class MathValidator
{
    public static bool IsEven(int n) => n % 2 == 0;

    public static bool IsPrime(int n)
    {
        if (n < 2) return false;
        for (int i = 2; i <= Math.Sqrt(n); i++)
            if (n % i == 0) return false;
        return true;
    }

    public static string Classify(int n)
    {
        if (n < 0) return "negative";
        else if (n == 0) return "zero";
        else return "positive";
    }
}

public class MathValidatorTests
{
    public static IEnumerable<object[]> EvenTestData =>
        new object[][]
        {
            new object[] {2, true},
            new object[] {3, false},
            new object[] {0, true},
            new object[] {-4, true},
            new object[] {7, false}
        };

    [Theory]
    public void IsEven_WithMemberData([MemberData(nameof(EvenTestData))object[] data) 
    {
        int n = (int)data[0];
        bool expected = (bool)data[1];
        MathValidator.IsEven(n).Should().Be(expected);
    }

    [Theory]
    [InlineData(2, true)]
    public void IsPrime_WithInlineData(int n, bool expected)
    {
        MathValidator.IsPrime(n).Should().Be(expected);
    }

    [Theory]
    [InlineData(-5, "negative")]
    [InlineData(0, "zero")]
    [InlineData(3, "positive")]
    public void Classify_WithInlineData(int n, string expected)
    {
        MathValidator.Classify(n).Should().Be(expected);
    }
}

<code>
public static class MathValidator
{
    public static bool IsEven(int n) => n % 2 == 0;

    public static bool IsPrime(int n)
    {
        if (n < 2) return false;
        for (int i = 2; i <= Math.Sqrt(n); i++)
            if (n % i == 0) return false;
        return true;
    }

    public static string Classify(int n)
    {
        if (n < 0) return "negative";
        else if (n == 0) return "zero";
        else return "positive";
    }
}

public class MathValidatorTests
{
    public static IEnumerable<object[]> EvenTestData =>
        new object[][]
        {
            new object[] {2, true},
            new object[] {3, false},
            new object[] {0, true},
            new object[] {-4, true},
            new object[] {7, false}
        };

    [Theory]
    public void IsEven_WithMemberData([MemberData(nameof(EvenTestData))object[] data) 
    {
        int n = (int)data[0];
        bool expected = (bool)data[1];
        MathValidator.IsEven(n).Should().Be(expected);
    }

    [Theory]
    [InlineData(2, true)]
    public void IsPrime_WithInlineData(int n, bool expected)
    {
        MathValidator.IsPrime(n).Should().Be(expected);
    }

    [Theory]
    [InlineData(-5, "negative")]
    [InlineData(0, "zero")]
    [InlineData(3, "positive")]
    public void Classify_WithInlineData(int n, string expected)
    {
        MathValidator.Classify(n).Should().Be(expected);
    }
}

<code>
public static class MathValidator
{
    public static bool IsEven(int n) => n % 2 == 0;

    public static bool IsPrime(int n)
    {
        if (n < 2) return false;
        for (int i = 2; i <= Math.Sqrt(n); i++)
            if (n % i == 0) return false;
        return true;
    }

    public static string Classify(int n)
    {
        if (n < 0) return "negative";
        else if (n == 0) return "zero";
        else return "positive";
    }
}

public class MathValidatorTests
{
    public static IEnumerable<object[]> EvenTestData =>
        new object[][]
        {
            new object[] {2, true},
            new object[] {3, false},
            new object[] {0, true},
            new object[] {-4, true},
            new object[] {7, false}
        };

    [Theory]
    public void IsEven_WithMemberData([MemberData(nameof(EvenTestData))object[] data) 
    {
        int n = (int)data[0];
        bool expected = (bool)data[1];
        MathValidator.IsEven(n).Should().Be(expected);
    }

    [Theory]
    [InlineData(2, true)]
    public void IsPrime_WithInlineData(int n, bool expected)
    {
        MathValidator.IsPrime(n).Should().Be(expected);
    }

    [Theory]
    [InlineData(-5, "negative")]
    [InlineData(0, "zero")]
    [InlineData(3, "positive")]
    public void Classify_WithInlineData(int n, string expected)
    {
        MathValidator.Classify(n).Should().Be(expected);
    }
}

<code>
public static class MathValidator
{
    public static bool IsEven(int n) => n % 2 == 0;

    public static bool IsPrime(int n)
    {
        if (n < 2) return false;
        for (int i = 2; i <= Math.Sqrt(n); i++)
            if (n % i == 0) return false;
        return true;
    }

    public static string Classify(int n)
    {
        if (n < 0) return "negative";
        else if (n == 0) return "zero";
        else return "positive";
    }
}

public class MathValidatorTests
{
    public static IEnumerable<object[]> EvenTestData =>
        new object[][]
        {
            new object[] {2, true},
            new object[] {3, false},
            new object[] {0, true},
            new object[] {-4, true},
            new object[] {7, false}
        };

    [Theory]
    public void IsEven_WithMemberData([MemberData(nameof(EvenTestData))object[] data) 
    {
        int n = (int)data[0];
        bool expected = (bool)data[1];
        MathValidator.IsEven(n).Should().Be(expected);
    }

    [Theory]
    [InlineData(2, true)]
    public void IsPrime_WithInlineData(int n, bool expected)
    {
        MathValidator.IsPrime(n).Should().Be(expected);
    }

    [Theory]
    [InlineData(-5, "negative")]
    [InlineData(0, "zero")]
    [InlineData(3, "positive")]
    public void Classify_WithInlineData(int n, string expected)
    {
        MathValidator.Classify(n).Should().Be(expected);
    }
}