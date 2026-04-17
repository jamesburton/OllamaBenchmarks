using System;
using System.Collections.Generic;

public class MonitoringOptions
{
    public bool EnableMetrics { get; set; } = true;
    public bool EnableTracing { get; set; } = true;
    public string MetricsEndpoint { get; set; } = "/metrics";
    public string ServiceName { get; set; } = "my-service";
}

public static class MonitoringHelper
{
    public static IEnumerable<string> GetEnabledFeatures(MonitoringOptions options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (options.EnableMetrics)
        {
            yield return "metrics";
        }

        if (options.EnableTracing)
        {
            yield return "tracing";
        }
    }
}