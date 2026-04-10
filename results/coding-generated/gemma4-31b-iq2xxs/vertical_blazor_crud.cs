*   Goal: Create a complete Todo list feature in a single C# file.
    *   Components:
        1.  `TodoItem` record.
        2.  `ITodoService` interface.
        3.  `TodoService` implementation (in-memory).
        4.  `TodoListBase` (ComponentBase code-behind).
        5.  xUnit v3 tests for `TodoService`.
        6.  bUnit tests for `TodoListBase`.
    *   Constraints:
        *   Use `Microsoft.AspNetCore.Components`.
        *   No other `using` statements (global usings are assumed).
        *   No namespaces.
        *   Use `Bunit.TestContext` to avoid ambiguity.
        *   Use `AwesomeAssertions` (`.Should()`).
        *   `TodoService` uses an instance field for ID counter.

    *   `TodoItem`: `public record TodoItem(int Id, string Title, bool IsCompleted);