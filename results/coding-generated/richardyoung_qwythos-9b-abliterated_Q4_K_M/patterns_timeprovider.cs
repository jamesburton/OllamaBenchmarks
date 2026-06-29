public class FakeTimeProvider : System.TimeProvider {
    private readonly DateTimeOffset fixedUtcNow;

    public FakeTimeProvider(DateTimeOffset fixedUtcNow) {
        this.fixedUtcNow = fixedUtcNow;
    }

    public override DateTimeOffset GetUtcNow() => fixedUtcNow;
}

public class ExpiryChecker {
    private readonly System.TimeProvider timeProvider;

    public ExpiryChecker(System.TimeProvider timeProvider) {
        this.timeProvider = timeProvider;
    }

    public bool IsExpired(DateTimeOffset expiresAt) => expiresAt < timeProvider.GetUtcNow();
}