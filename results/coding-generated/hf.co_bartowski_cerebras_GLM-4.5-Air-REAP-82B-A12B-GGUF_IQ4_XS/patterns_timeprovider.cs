public class FakeTimeProvider : System.TimeProvider
{
    private readonly DateTimeOffset _fixedUtcNow;

    public FakeTimeProvider(DateTimeOffset fixedUtcNow)
    {
        _fixedUtcNow = fixedUtcNow;
    }

    protected override System.DateTimeOffset GetUtcNow()
    {
        return _fixedUtcNow;
    }
}

public class ExpiryChecker
{
    private readonly System.TimeProvider _timeProvider;

    public ExpiryChecker(System.TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public bool IsExpired(System.DateTimeOffset expiresAt)
    {
        return expiresAt < _timeProvider.GetUtcNow();
    }
}