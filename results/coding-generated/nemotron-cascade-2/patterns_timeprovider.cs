class FakeTimeProvider : TimeProvider
{
    private readonly DateTimeOffset _fixedUtcNow;

    public FakeTimeProvider(DateTimeOffset fixedUtcNow)
    {
        _fixedUtcNow = fixedUtcNow;
    }

    public override DateTimeOffset GetUtcNow()
    {
        return _fixedUtcNow;
    }
}

class ExpiryChecker
{
    private readonly TimeProvider _timeProvider;

    public ExpiryChecker(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public bool IsExpired(DateTimeOffset expiresAt)
    {
        return expiresAt < _timeProvider.GetUtcNow();
    }
}