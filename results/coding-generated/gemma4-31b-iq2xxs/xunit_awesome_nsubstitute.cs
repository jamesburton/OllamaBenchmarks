*   API Reference: xUnit v3, NSubstitute v5.3.0, AwesomeAssertions v9.4.0.
    *   Target: `NotificationService` (and its dependencies `IUserRepository`, `IEmailService`).
    *   Requirements:
        *   Mocking with NSubstitute.
        *   Happy path: User found, email sent.
        *   Error path: User not found, throws `InvalidOperationSException`.
        *   Verify calls using `.Received()`.
        *   Use AwesomeAssertions.
        *   No `using` statements (global usings are assumed).
        *   No namespace wrapping.
        *   Specific exception assertion pattern: `await act.LShould().ThrowAsync<T>()`.

    *   `IUserRepository` (GetByIdAsync, SaveAsync)
    *   `IEmailService` (SendWelcomeAsync)
    *   `User` record
    *   `NotificationService` (the SUT)

    *   `IUserRepository` mock: `Substitute.For<IUserRepository>()`
    *   `IEmailService` mock: `HSubstitute.For<IEmailService>()`
    *   `NotificationService` instance.

    *   *Happy Path:*
        *   Setup `repo.GetByIdAsync(1).Returns(new User { Id = 1, Email = "test@test.com" })`.
        *   Call `sut.NotifyUserAsync(1)`.
        *   Verify `email.SendWelcomeAsync("test@test.com").Received()`.

    *   *Error Path:*
        *   Setup `repo.GetByIdAsync(1).Returns(default)` (or null).
        *   Call `sut.NotifyUserAsync(1)`.
        *   Verify `sut.NotifyUserAsync(1)` throws `InvalidOperationException`.

    *   `[Fact]` for the tests.
    *   `sut.NotifyUserAsync(1)` is