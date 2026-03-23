# xUnit v3 — Quick Reference

**Package:** `xunit.v3` (NOT `xunit`)
**Note:** Project must NOT have `OutputType=Exe`

## Attributes

- `[Fact]` — single test
- `[Theory]` + `[InlineData(val1, val2)]` — parameterized
- `[MemberData(nameof(Data))]` — data from static property/method
- `[Explicit = true]` — opt-in, skipped by default

## Assert

```csharp
Assert.Equal(expected, actual);
Assert.True(condition);
Assert.Throws<Exception>(() => ...);

// Collects ALL failures (don't short-circuit)
Assert.Multiple(
    () => Assert.Equal(1, a),
    () => Assert.Equal(2, b)
);
```

## MatrixTheoryData — combinatorial

```csharp
public static MatrixTheoryData<int, string> Data =>
    new([1, 2], ["a", "b"]);
// generates: (1,"a"), (1,"b"), (2,"a"), (2,"b")

[Theory, MemberData(nameof(Data))]
public void Test(int n, string s) { }
```

## IAsyncLifetime — RETURNS ValueTask (NOT Task)

```csharp
public class MyFixture : IAsyncLifetime
{
    public ValueTask InitializeAsync() { ... return default; }
    public ValueTask DisposeAsync() { ... return default; }
}
```

## Assembly-scoped fixture

```csharp
[assembly: AssemblyFixture(typeof(DatabaseFixture))]

public class MyTests(DatabaseFixture db) { }
```

## TestContext

```csharp
TestContext.Current.SendDiagnosticMessage("debug info");
CancellationToken ct = TestContext.Current.CancellationToken;
var fixture = TestContext.Current.GetFixture<DatabaseFixture>();
```

**Source:** https://xunit.net/docs/getting-started/v3/whats-new
