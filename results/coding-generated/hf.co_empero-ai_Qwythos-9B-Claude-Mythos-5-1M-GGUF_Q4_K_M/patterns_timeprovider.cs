public class FakeTimeProvider : TimeProvider
{
    private readonly DateTimeOffset _fixedUtcNow;

    public FakeTimeProvider(DateTimeOffset fixedUtcNow)
    {
        _fixedUtcNow = fixedUtcNow;
    }

    public override DateTimeOffset GetUtcNow() => _fixedUtcNow;
}

public class ExpiryChecker(TimeProvider timeProvider)
{
    private readonly TimeProvider _timeProvider = timeProvider;

    public bool IsExpired(DateTimeOffset expiresAt) => expiresAt < _timeProvider.GetUtcNow();
}