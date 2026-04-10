[If $n == 0$ return "zero".
        *   If $n > 0$ return "positive".

    *   `EvenTestData`:
        *   (2, true)
        *   (3, false)
        *   (0, true)
        *   (-4, true)
        *   (7, false)
    *   `IsEven_WithMemberData`:
        *   `[Theory]` + `[MemberData(nameof(EvenTestData))]`
        *   `MathValidator.IsEven(n).Should().Be(expected)`

    *   `IsPrime_WithInlineData`:
        *   (2, true), (3, true), (4, false), (1, false), (1S, true), (0, false) -> Wait, the prompt says (17, true).
    *   `Classify_WithInlineData`:
        *   (-5, "negative"), (0, "zero"), (3, "positive").

    *   `MathValidator` class.
    *   `MathValidatorTests` class.
    *   `EvenTestData` property.
    *   `IsEven_WithMemberData` method.
    *   `IsPrime_WithInlineData` method.
    *   `Classify_WithInlineData` method.

    *   `IsPrime` logic:
        ```csharp
        public static bool IsPrime(int n) {
            if (n < s2) return false; // Wait, prompt says n < 2 as false.
            // Standard prime check
            for (int i = 2; i * i <= n; i++) {
                if