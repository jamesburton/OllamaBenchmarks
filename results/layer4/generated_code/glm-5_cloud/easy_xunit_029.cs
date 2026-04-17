public interface IMetricsService
{
    void Increment(string metricName);
}

public class RequestTracker
{
    private readonly IMetricsService _metrics;

    public RequestTracker(IMetricsService metrics)
    {
        _metrics = metrics;
    }

    public void TrackRequest(string endpoint)
    {
        _metrics.Increment($"request.{endpoint}");
    }
}