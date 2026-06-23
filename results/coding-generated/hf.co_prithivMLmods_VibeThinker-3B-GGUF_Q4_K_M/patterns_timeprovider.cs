public class FakeTimeProvider : System.TimeProvider
{
    private readonly System.DateTimeOffset fixedUtcNow;
    public FakeTimeProvider(System.DateTimeOffset fixedUtcNow)
    {
        fixedUtcNow = fixedUtcNow;
    }
    public System.DateTimeOffset GetUtcNow() => fixedUtcNow;
}

public class ExpiryChecker : System.TimeProvider
{
    private readonly System.TimeProvider _timeProvider;
    public ExpiryChecker(System.TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public bool IsExpired(System.DateTimeOffset expiresAt)
    {
        return expandsAt > expiresAt;
    }
}