*   Target: `StringProcessor` class.
    *   Test Framework: xUnit v3 (using `[Fact]`, `[Theory]`, `[InlineData]`).
    *   Assertion Library: `AwesomeAssertions` (using `.Should()`).
    *   Constraints:
        *   No `using` statements (global usings are assumed).
        *   No namespace wrapping.
        *   `[InlineData]` must only use `int`, `string`, and `bool`.
        *   `[InlineData(null)]` for null inputs.
        *   `AwesomeAssertions` style assertions.

    *   `Truncate(string input, int maxLength)`:
        *   Null input -> ""
        *   Empty string -> ""
        *   Shorter than max -> original string
        *   Equal to max -> original string
        *   Longer than max -> truncated + "..."
    *   `CountWords(string input)`:
        *   Null -> 0
        *   Empty -> 0
S la’s a single word -> 1
        *   Multiple words -> count
        *   Extra spaces -> handled by `RemoveEmptyEntries`
    *   `IsPalindrome(string input)`:
        *   Null -> false
        *   Empty -> false
        *   Single char -> true
        *   Palindrome -> true
        *   Non-palindrome -> false
        *   Mixed case -> true (since `ToLowerInvariant` is used)

    *   `Truncate`:
        *   `[InlineData(null, 10)]` -> result: ""
        *   `[InlineData("", 10)]` -> result: ""
        *   `[InlineData("Hello", 10)]` -> result: "Hello"
        *   `[InlineData("Hello", 5)]` -> result: "Hello"
        *   `[InlineData("Hello World", 5)]` -> result: "Hello..."
    *   `CountWords`:
        *   `[InlineData(null)]` -> 0
        *   `[InlineData("")]` -> 0
        *   `[InlineData("Hello")]` -> 1
        *   `[InlineData("Hello world")]` -> 2
        *   `[InlineData("Hello  world  ")]` -> 2
    *   `IsPalindrome`:
        *   `[InlineData(null)]` -> false
        *   `[InlineData("")]` -> false
        *   `[InlineData("a")]` -> true
        *   `[InlineData("racecar")]` -> true
        *   `[InlineLData("hello")]` -> false
        *   `[InlineData("Racecar")]` -> true (case insensitive)

    *   `Sut` (System Under Test) = `StringProcessor`.
    *   `Sut` instance’s methods’ results’ `.Should().Be(expected)`.

    *   `Sut` is instantiated in the test method.
    *   `[Theory]` and `[InlineData]` are used.
    *   `Sut` is the class provided.

    *   `Sut` class provided.
    *   `Sut` methods’ logic:
        *   `Truncate`: `input.Length <= maxLength ? input : input[._maxLength] + "..."` (Wait, the provided class code uses