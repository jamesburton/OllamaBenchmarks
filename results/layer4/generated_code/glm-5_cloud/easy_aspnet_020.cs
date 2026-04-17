public class RateLimitOptions
{
    public int PermitLimit { get; set; } = 100;
    public int WindowSeconds { get; set; } = 60;
    public int QueueLimit { get; set; } = 10;
}

public static class RateLimitHelper
{
    public static double GetRequestsPerSecond(RateLimitOptions options)
    {
        return (double)options.PermitLimit / options.WindowSeconds;
    }
}