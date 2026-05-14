public class FakeTimeProvider : TimeProvider
{
    private readonly DateTimeOffset fixedUtcNow;

    public FakeTimeProvider(DateTimeOffset fixedUtcNow)
    {
        this.fixedUtcNow = fixedUtcNow;
    }

    public override DateTimeOffset GetUtcNow() => fixedUtcNow;
}

public class ExpiryChecker
{
    private readonly TimeProvider timeProvider;

    public ExpiryChecker(TimeProvider timeProvider)
    {
        this.timeProvider = timeProvider;
    }

    public bool IsExpired(DateTimeOffset expiresAt) => expiresAt < timeProvider.GetUtcNow();
}