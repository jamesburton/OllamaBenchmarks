public class FakeTimeProvider : global::System.TimeProvider
{
    private readonly global::System.DateTimeOffset _fixedUtcNow;

    public FakeTimeProvider(global::System.DateTimeOffset fixedUtcNow)
    {
        _fixedUtcNow = fixedUtcNow;
    }

    public override global::System.DateTimeOffset GetUtcNow()
    {
        return _fixedUtcNow;
    }
}

public class ExpiryChecker
{
    private readonly global::System.TimeProvider _timeProvider;

    public ExpiryChecker(global::System.TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public bool IsExpired(global::System.DateTimeOffset expiresAt)
    {
        return expiresAt < _timeProvider.GetUtcNow();
    }
}