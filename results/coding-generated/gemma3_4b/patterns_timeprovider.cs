public class FakeTimeProvider : TimeProvider
{
    private DateTimeOffset _utcNow;

    public FakeTimeProvider(DateTimeOffset fixedUtcNow)
    {
        _utcNow = fixedUtcNow;
    }

    public override DateTimeOffset GetUtcNow()
    {
        return _utcNow;
    }
}

public class ExpiryChecker : TimeProvider
{
    private TimeProvider _timeProvider;

    public ExpiryChecker(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public bool IsExpired(DateTimeOffset expiresAt)
    {
        return expiresAt.UtcDateTime < _timeProvider.GetUtcNow().UtcDateTime;
    }
}