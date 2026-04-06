class FakeTimeProvider : TimeProvider
{
    private DateTimeOffset _fixedUtcNow;

    public FakeTimeProvider(DateTimeOffset fixedUtcNow) => _fixedUtcNow = fixedUtcNow;

    public override DateTimeOffset GetUtcNow() => _fixedUtcNow;
}

class ExpiryChecker
{
    private TimeProvider _timeProvider;

    public ExpiryChecker(TimeProvider timeProvider) => _timeProvider = timeProvider;

    public bool IsExpired(DateTimeOffset expiresAt) => expiresAt < TimeProvider.GetUtcNow();
}