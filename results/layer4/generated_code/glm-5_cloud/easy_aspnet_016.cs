public class CacheOptions
{
    public int AbsoluteExpirationSeconds { get; set; } = 3600;
    public int SlidingExpirationSeconds { get; set; } = 600;
    public bool Enabled { get; set; } = true;
}

public static class CacheOptionsHelper
{
    public static TimeSpan GetAbsoluteExpiration(CacheOptions options)
    {
        return TimeSpan.FromSeconds(options.AbsoluteExpirationSeconds);
    }
}