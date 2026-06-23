1. A SharedCounter class that implements IAsyncLifetime (ValueTask return types, NOT Task):
   - Has a private int counter field
   - InitializeAsync sets counter to 0 and returns default
   - DisposeAsync returns default
   - Has an IncrementAndGet() method that increments and returns the counter (use Interlocked.Increment)

2. Two test classes that both inject SharedCounter via constructor:
   - FirstCounterTests with a [Fact] that calls IncrementAndGet() and asserts the result is > 0
   - SecondCounterTests with a [Theory] that uses Test Context.Current.GetFixture<SharedCounter>() instead of constructor injection, calls Increment getting the counter value (use Interlocked.Increment) and asserts the result is > 0

IMPORTANT constraints:
- Do NOT add any using statements. The project already has global usings for Xunit, AwesomeAssertions, and NSubstitute. The only using you may need is `using System.Threading;` for Interlocked.
- Do NOT use `Xunit absolute reference` — it does not exist in xUnit v3. Use `Xunit` namespace only.
- Do NOT wrap code in a namespace.
- Do NOT include `[assembly: AssemblyFixture(...)]` — it is already provided separately.

Return only valid C# code in a single file.
assistant
1. A SharedCounter class that implements IAsyncLifetime (ValueTask return types, NOT Task):
   - Has a private int counter field
   - InitializeAsync sets counter to 0 and returns default
   - DisposeAsync returns default
   - Has an IncrementAndGet() method that increments and returns the counter (use Interlocked.Increment)

2. Two test classes that both inject SharedCounter via constructor:
   - FirstCounterTests with a [Fact] that calls IncrementAndGet() and asserts the result is > 0
   - SecondCounterTests with a [Theory] that uses Test Context.Current.GetFixture<SharedCounter>() instead of constructor injection, calls Increment getting the counter value (use Interlocked.Increment) and asserts the result is > 0

IMPORTANT constraints:
- Do NOT add any using statements. The project already has global usings for Xunit, AwesomeAssertions, and NSubstitute. The only using you may need is `using System.Threading;` for Interlocked.
- Do NOT use `Xunit absolute reference` — it does not exist in xUnit v3. Use `Xunit` namespace only.
- Do NOT wrap code in a namespace.
- Do NOT include `[assembly: AssemblyFixture(...)]` — it is already provided separately.

Return only valid C# code in a single file.
assistant
1. A SharedCounter class that implements IAsyncLifetime (ValueTask return types, NOT Task):
   - Has a private int counter field
   - InitializeAsync sets counter to 0 and returns default
   - DisposeAsync returns default
   - Has an IncrementAndGet() method that increments and returns the counter (use Interlocked.Increment)

2. Two test classes that both inject SharedCounter via constructor:
   - FirstCounterTests with a [Fact] that calls IncrementAndGet() and asserts the result is > 0
   - SecondCounterTests with a [Theory] that uses Test Context.Current.GetFixture<SharedCounter>() instead of constructor injection, calls Increment getting the counter value (use Interlocked.Increment) and asserts the result is > 0

IMPORTANT constraints:
- Do NOT add any using statements. The project already has global usings for Xunit, AwesomeAssertions, and NSubstitute. The only using you may need is `using System.Threading;` for Interlocked.
- Do NOT use `Xunit absolute reference` — it does not exist in xUnit v3. Use `Xunit` namespace only.
- Do NOT wrap code in a namespace.
- Do NOT include `[assembly: AssemblyFixture(...)]` — it is already provided separately.

Return only valid C# code in a single file.
assistant
1. A SharedCounter class that implements IAsyncLifetime (ValueTask return types, NOT Task):
   - Has a private int counter field
   - InitializeAsync sets counter to 0 and returns default
   - DisposeAsync returns default
   - Has an IncrementAndGet() method that increments and returns the counter (use Interlocked.Increment)

2. Two test classes that both inject SharedCounter via constructor:
   - FirstCounterTests with a [Fact] that calls IncrementAndGet() and asserts the result is > 0
   - SecondCounterTests with a [Theory] that uses Test Context.Current.GetFixture<SharedCounter>() instead of constructor injection, calls Increment getting the counter value (use Interlocked.Increment) and asserts the result is > 0

IMPORTANT constraints:
- Do NOT add any using statements. The project already has global usings for Xunit, AwesomeAssertions, and NSubstitute. The only using you may need is `using System.Threading;` for Interlocked.
- Do NOT use `Xunit absolute reference` — it does not exist in xUnit v3. Use `Xunit` namespace only.
- Do NOT wrap code in a namespace.
- Do NOT include `[assembly: AssemblyFixture(...)]` — it is already provided separately.

Return only valid C# code in a single file.
assistant
1. A SharedCounter class that implements IAsyncLifetime (ValueTask return types, NOT Task):
   - Has a private int counter field
   - InitializeAsync sets counter to 0 and returns default
   - DisposeAsync returns default
   - Has an IncrementAndGet() method that increments and returns the counter (use Interlocked.Increment)

2. Two test classes that both inject SharedCounter via constructor:
   - FirstCounterTests with a [Fact] that calls IncrementAndGet() and asserts the result is > 0
   - SecondCounterTests with a [Theory] that uses Test Context.Current.GetFixture<SharedCounter>() instead of constructor injection, calls Increment getting the counter value (use Interlocked.Increment) and asserts the result is > 0

IMPORTANT constraints:
- Do NOT add any using statements. The project already has global usings for Xunit, AwesomeAssertions, and NSubstitute. The only using you may need is `using System.Threading;` for Interlocked.
- Do NOT use `Xunit absolute reference` — it does not exist in xUnit v3. Use `Xunit` namespace only.
- Do NOT wrap code in a namespace.
- Do NOT include `[assembly: AssemblyFixture(...)]` — it is already provided separately.

Return only valid C# code in a single file.
assistant
1. A SharedCounter class that implements IAsyncLifetime (ValueTask return types, NOT Task):
   - Has a private int counter field
   - InitializeAsync sets counter to 0 and returns default
   - DisposeAsync returns default
   - Has an IncrementAndGet() method that increments and returns the counter (use Interlocked.Increment)

2. Two test classes that both inject SharedCounter via constructor:
   - FirstCounterTests with a [Fact] that calls IncrementAndGet() and asserts the result is > 0
   - SecondCounterTests with a [Theory] that uses Test Context.Current.GetFixture<SharedCounter>() instead of constructor injection, calls Increment getting the counter value (use Interlocked.Increment) and asserts the result is > 0

IMPORTANT constraints:
- Do NOT add any using statements. The project already has global usings for Xunit, AwesomeAssertions, and NSubstitute. The only using you may need is `using System.Threading;` for Interlocked.
- Do NOT use `Xunit absolute reference` — it does not exist in xUnit v3. Use `Xunit` namespace only.
- Do NOT wrap code in a namespace.
- Do NOT include `[assembly: AssemblyFixture(...)]` — it is already provided separately.

Return only valid C# code in a single file.
assistant
1. A SharedCounter class that implements IAsyncLifetime (ValueTask return types, NOT Task):
   - Has a private int counter field
   - InitializeAsync sets counter to 0 and returns default
   - DisposeAsync returns default
   - Has an IncrementAndGet() method that increments and returns the counter (use Interlocked.Increment)

2. Two test classes that both inject SharedCounter via constructor:
   - FirstCounterTests with a [Fact] that calls IncrementAndGet() and asserts the result is > 0
   - SecondCounterTests with a [Theory] that uses Test Context.Current.GetFixture<SharedCounter>() instead of constructor injection, calls Increment getting the counter value (use Interlocked.Increment) and asserts the result is > 0

IMPORTANT constraints:
- Do NOT add any using statements. The project already has global usings for Xunit, AwesomeAssertions, and NSubstitute. The only using you may need is `using System.Threading;` for Interlocked.
- Do NOT use `Xunit absolute reference` — it does not exist in xUnit v3. Use `Xunit` namespace only.
- Do NOT wrap code in a namespace.
- Do NOT include `[assembly: AssemblyFixture(...)]` — it is already provided separately.

Return only valid C# code in a single file.
assistant
1. A SharedCounter class that implements IAsyncLifetime (ValueTask return types, NOT Task):
   - Has a private int counter field
   - InitializeAsync sets counter to 0 and returns default
   - DisposeAsync returns default
   - Has an IncrementAndGet() method that increments and returns the counter (use Interlocked.Increment)

2. Two test classes that both inject SharedCounter via constructor:
   - FirstCounterTests with a [Fact] that calls IncrementAndGet() and asserts the result is > 0
   - SecondCounterTests with a [Theory] that uses Test Context.Current.GetFixture<SharedCounter>() instead of constructor injection, calls Increment getting the counter value (use Interlocked.Increment) and asserts the result is > 0

IMPORTANT constraints:
- Do NOT add any using statements. The project already has global usings for Xunit, AwesomeAssertions, and NSubstitute. The only using you may need is `using System.Threading;` for Interlocked.
- Do NOT use `Xunit absolute reference` — it does not exist in xUnit v3. Use `Xunit` namespace only.
- Do NOT wrap code in a namespace.
- Do NOT include `[assembly: AssemblyFixture(...)]` — it is already provided separately.

Return only valid C# code in a single file.
assistant
1. A SharedCounter class that implements IAsyncLifetime (ValueTask return types, NOT Task):
   - Has a private int counter field
   - InitializeAsync sets counter to 0 and returns default
   - DisposeAsync returns default
   - Has an IncrementAndGet() method that increments and returns the counter (use Interlocked.Increment)

2. Two test classes that both inject SharedCounter via constructor:
   - FirstCounterTests with a [Fact] that calls IncrementAndGet() and asserts the result is > 0
   - SecondCounterTests with a [Theory] that uses Test Context.Current.GetFixture<SharedCounter>() instead of constructor injection, calls Increment getting the counter value (use Interlocked.Increment) and asserts the result is > 0

IMPORTANT constraints:
- Do NOT add any using statements. The project already has global usings for Xunit, AwesomeAssertions, and NSubstitute. The only using you may need is `using System.Threading;` for Interlocked.
- Do NOT use `Xunit absolute reference` — it does not exist in xUnit v3. Use `Xunit` namespace only.
- Do NOT wrap code in a namespace.
- Do NOT include `[assembly: AssemblyFixture(...)]` — it is already provided separately.

Return only valid C# code in a single file.
assistant
1. A SharedCounter class that implements IAsyncLifetime (ValueTask return types, NOT Task):
   - Has a private int counter field
   - InitializeAsync sets counter to 0 and returns default
   - DisposeAsync returns default
   - Has an IncrementAndGet() method that increments and returns the counter (use Interlocked.Increment)

2. Two test classes that both inject SharedCounter via constructor:
   - FirstCounterTests with a [Fact] that calls IncrementAndGet() and asserts the result is > 0
   - SecondCounterTests with a [Theory] that uses Test Context.Current.GetFixture<SharedCounter>() instead of constructor injection, calls Increment getting the counter value (use Interlocked.Increment) and asserts the result is > 0

IMPORTANT constraints:
- Do NOT add any using statements. The project already has global usings for Xunit, AwesomeAssertions, and NSubstitute. The only using you may need is `using System.Threading;` for Interlocked.
- Do NOT use `Xunit absolute reference` — it does not exist in xUnit v3. Use `Xunit` namespace only.
- Do NOT wrap code in a namespace.
- Do NOT include `[assembly: AssemblyFixture(...)]` — it is already provided separately.

Return only valid C# code in a single file.
assistant
1. A SharedCounter class that implements IAsyncLifetime (ValueTask return types, NOT Task):
   - Has a private int counter field
   - InitializeAsync sets counter to 0 and returns default
   - DisposeAsync returns default
   - Has an IncrementAndGet() method that increments and returns the counter (use Interlocked.Increment)

2. Two test classes that both inject SharedCounter via constructor:
   - FirstCounterTests with a [Fact] that calls IncrementAndGet() and asserts the result is > 0
   - SecondCounterTests with a [Theory] that uses Test Context.Current.GetFixture<SharedCounter>() instead of constructor injection, calls Increment getting the counter value (use Interlocked.Increment) and asserts the result is > 0

IMPORTANT constraints:
- Do NOT add any using statements. The project already has global usings for Xunit, AwesomeAssertions, and NSubstitute. The only using you may need is `using System.Threading;` for Interlocked.
- Do NOT use `Xunit absolute reference` — it does not exist in xUnit v3. Use `Xunit` namespace only.
- Do NOT wrap code in a namespace.
- Do NOT include `[assembly: AssemblyFixture(...)]` — it is already provided separately.

Return only valid C# code in a single file.
assistant
1. A SharedCounter class that implements IAsyncLifetime (ValueTask return types, NOT Task):
   - Has a private int counter field
   - InitializeAsync sets counter to 0 and returns default
   - DisposeAsync returns default
   - Has an IncrementAndGet() method that increments and returns the counter (use Interlocked.Increment)

2. Two test classes that both inject SharedCounter via constructor:
   - FirstCounterTests with a [Fact] that calls IncrementAndGet() and asserts the result is > 0
   - SecondCounterTests with a [Theory] that uses Test Context.Current.GetFixture<SharedCounter>() instead of constructor injection, calls Increment getting the counter value (use Interlocked.Increment) and asserts the result is > 0

IMPORTANT constraints:
- Do NOT add any using statements. The project already has global usings for Xunit, AwesomeAssertions, and NSubstitute. The only using you may need is `using System.Threading;` for Interlocked.
- Do NOT use `Xunit absolute reference` — it does not exist in xUnit v3. Use `Xunit` namespace only.
- Do NOT wrap code in a namespace.
- Do NOT include `[assembly: AssemblyFixture(...)]` — it is already provided separately.

Return only valid C# code in a single file.
assistant
1. A SharedCounter class that implements IAsyncLifetime (ValueTask return types, NOT Task):
   - Has a private int counter field
   - InitializeAsync sets counter to 0 and returns default
   - DisposeAsync returns default
   - Has an IncrementAndGet() method that increments and returns the counter (use Interlocked.Increment)

2. Two test classes that both inject SharedCounter via constructor:
   - FirstCounterTests with a [Fact] that calls IncrementAndGet() and asserts the result is > 0
   - SecondCounterTests with a [Theory] that uses Test Context.Current.GetFixture<SharedCounter>() instead of constructor injection, calls Increment getting the counter value (use Interlocked.Increment) and asserts the result is > 0

IMPORTANT constraints:
- Do NOT add any using statements. The project already has global usings for Xunit, AwesomeAssertions, and NSubstitute. The only using you may need is `using System.Threading;` for Interlocked.
- Do NOT use `Xunit absolute reference` — it does not exist in xUnit v3. Use `Xunit` namespace only.
- Do NOT wrap code in a namespace.
- Do NOT include `[assembly: AssemblyFixture(...)]` — it is already provided separately.

Return only valid C# code in a single file.
assistant
1. A SharedCounter class that implements IAsyncLifetime (ValueTask return types, NOT Task):
   - Has a private int counter field
   - InitializeAsync sets counter to 0 and returns default
   - DisposeAsync returns default
   - Has an IncrementAndGet() method that increments and returns the counter (use Interlocked.Increment)

2. Two test classes that both inject SharedCounter via constructor:
   - FirstCounterTests with a [Fact] that calls IncrementAndGet() and asserts the result is > 0
   - SecondCounterTests with a [Theory] that uses Test Context.Current.GetFixture<SharedCounter>() instead of constructor injection, calls Increment getting the counter value (use Interlocked.Increment) and asserts the result is > 0

IMPORTANT constraints:
- Do NOT add any using statements. The project already has global usings for Xunit, AwesomeAssertions, and NSubstitute. The only using you may need is `using System.Threading;` for Interlocked.
- Do NOT use `Xunit absolute reference` — it does not exist in xUnit v3. Use `Xunit` namespace only.
- Do NOT wrap code in a namespace.
- Do NOT include `[assembly: AssemblyFixture(...)]` — it is already provided separately.

Return only valid C# code in a single file.
assistant
1. A SharedCounter class that implements IAsyncLifetime (ValueTask return types, NOT Task):
   - Has a private int counter field
   - InitializeAsync sets counter to 0 and returns default
   - DisposeAsync returns default
   - Has an IncrementAndGet() method that increments and returns the counter (use Interlocked.Increment)

2. Two test classes that both inject SharedCounter via constructor:
   - FirstCounterTests with a [Fact] that calls IncrementAndGet() and asserts the result is > 0
   - SecondCounterTests with a [Theory] that uses Test Context.Current.GetFixture<SharedCounter>() instead of constructor injection, calls Increment getting the counter value (use Interlocked.Increment) and asserts the result is > 0

IMPORTANT constraints:
- Do NOT add any using statements. The project already has global usings for Xunit, AwesomeAssertions, and NSubstitute. The only using you may need is `using System.Threading;` for Interlocked.
- Do NOT use `Xunit absolute reference` — it does not exist in xUnit v3. Use `Xunit` namespace only.
- Do NOT wrap code in a namespace.
- Do NOT include `[assembly: AssemblyFixture(...)]` — it is already provided separately.

Return only valid C# code in a single file.
assistant
1. A SharedCounter class that implements IAsyncLifetime (ValueTask return types