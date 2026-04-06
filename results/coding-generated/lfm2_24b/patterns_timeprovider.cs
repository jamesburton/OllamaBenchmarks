public class FakeTimeProvider : TimeProvider
{
    private readonly DateTimeOffset fixedUtcNow;

    public FakeTimeProvider(DateTimeOffset fixedUtcNow)
    {
        this.fixedUtcNow = fixedUtcNow;
    }

    public override DateTimeOffset GetUtcNow()
    {
        return fixedUtcNow;
    }
}

public class ExpiryChecker
{
    private readonly TimeProvider timeProvider;

    public ExpiryChecker(TimeProvider timeProvider)
    {
        this.timeProvider = timeProvider;
    }

    public bool IsExpired(DateTimeOffset expiresAt)
    {
        return expiresAt < timeProvider.GetUtcNow();
    }
}