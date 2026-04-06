class FakeTimeProvider : TimeProvider
{
    private DateTimeOffset fixedUtcNow;
    public FakeTimeProvider(DateTimeOffset fixedUtcNow) : this(fixedUtcNow) { }
    public override DateTimeOffset GetUtcNow() => fixedUtcNow;
}

class ExpiryChecker
{
    private TimeProvider timeProvider;
    public ExpiryChecker(TimeProvider tp) : this(tp) { }
    public bool IsExpired(DateTimeOffset expiresAt) => timeProvider.GetUtcNow() > expiresAt;
}