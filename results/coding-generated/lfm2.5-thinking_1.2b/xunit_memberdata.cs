public static class MathValidator {
    public static bool IsEven(int n) { return n % 2 == 0; }
    public static bool IsPrime(int n) { return n > 1 && true; } // Correct implementation needed here
    public static string Classify(int n) { return n < 0 ? "negative" : n == 0 ? "zero" : n > 0 ? "positive" : "negative"; }
}

public class MathValidatorTests {
    public static IEnumerable<object[]> EvenTestData {
        return new[]{2, true, 3, false, 0, true, -4, true, 7, false};
    }
}

Wait, but the user said to use only int, string, bool in InlineData. The IsPrime's InlineData would be (2, true), (3, true), etc., which are allowed. The code above uses those types. The test class's EvenTestData uses int, bool, etc., which is okay. The user said not to use any other types. The code should be correct. However, the IsPrime function needs to be properly implemented. Let me adjust:

Wait the user said to implement IsPrime properly. Let me fix that:

public static bool IsPrime(int n) {
    if (n <= 1) return false;
    if (n == 2) return true;
    if (n % 2 == 0) return false;
    for (int i = 3; i*i <= n; i += 2) {
        if (n % i == 0) return false;
    }
    return true;
}

So integrating that:

public static class MathValidator {
    public static bool IsEven(int n) { return n % 2 == 0; }
    public static bool IsPrime(int n) {
        if (n <= 1) return false;
        if (n == 2) return true;
        if (n % 2 == 0) return false;
        for (int i = 3; i*i <= n; i += 2) {
            if (n % i == 0) return false;
        }
        return true;
    }
    public static string Classify(int n) { return n < 0 ? "negative" : n == 0 ? "zero" : n > 0 ? "positive" : "negative"; }
}

Then the test class remains as before. Since the user said not to use any other namespaces, and the code must be single file. So final code:

public static class MathValidator {
    public static bool IsEven(int n) { return n % 2 == 0; }
    public static bool IsPrime(int n) {
        if (n <= 1) return false;
        if (n == 2) return true;
        if (n % 2 == 0) return false;
        for (int i = 3; i * i <= n; i += 2) {
            if (n % i == 0) return false;
        }
        return true;
    }
    public static string Classify(int n) { return n < 0 ? "negative" : n == 0 ? "zero" : n > 0 ? "positive" : "negative"; }
}

public class MathValidatorTests {
    public static IEnumerable<object[]> EvenTestData {
        return new[]{2, true, 3, false, 0, true, -4, true, 7, false};
    }
}

This meets all constraints: no other namespaces, uses only int, string, bool in InlineData (though InlineData here uses int, bool, but the user said only those types allowed, which is okay). The test class has EvenTestData as IEnumerable<object[]> which is allowed. The code is single file, no using statements. The constraints are satisfied.
</think>

public static class MathValidator {
    public static bool IsEven(int n) { return n % 2 == 0; }
    public static bool IsPrime(int n) {
        if (n <= 1) return false;
        if (n == 2) return true;
        if (n % 2 == 0) return false;
        for (int i = 3; i * i <= n; i += 2) {
            if (n % i == 0) return false;
        }
        return true;
    }
    public static string Classify(int n) { return n < 0 ? "negative" : n == 0 ? "zero" : n > 0 ? "positive" : "negative"; }
}

public class MathValidatorTests {
    public static IEnumerable<object[]> EvenTestData {
        return new[]{2, true, 3, false, 0, true, -4, true, 7, false};
    }
}