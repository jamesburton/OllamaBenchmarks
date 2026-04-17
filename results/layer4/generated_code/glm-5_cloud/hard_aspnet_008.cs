using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace ServiceSlice;

// Options configuration for the QueueProcessorService
public class QueueProcessorOptions
{
    public int BatchSize { get; set; } = 10;
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(5);
}

// Abstraction for the message queue infrastructure
public interface IMessageQueue
{
    Task<IReadOnlyList<string>> DequeueAsync(int count, CancellationToken ct);
    Task AcknowledgeAsync(IReadOnlyList<string> messageIds, CancellationToken ct);
}

// Abstraction for the business logic processing
public interface IMessageProcessor
{
    Task<bool> ProcessAsync(string message, CancellationToken ct);
}

// Background service implementation
public class QueueProcessorService : BackgroundService
{
    private readonly IMessageQueue _queue;
    private readonly IMessageProcessor _processor;
    private readonly QueueProcessorOptions _options;
    private readonly ILogger<QueueProcessorService> _logger;

    // Public counters for health checks and diagnostics
    // Using Interlocked for thread safety if accessed externally, though primarily updated here.
    private int _processedCount;
    public int ProcessedCount => _processedCount;

    private int _failedCount;
    public int FailedCount => _failedCount;

    public QueueProcessorService(
        IMessageQueue queue,
        IMessageProcessor processor,
        IOptions<QueueProcessorOptions> options,
        ILogger<QueueProcessorService> logger)
    {
        _queue = queue ?? throw new ArgumentNullException(nameof(queue));
        _processor = processor ?? throw new ArgumentNullException(nameof(processor));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Queue Processor Service starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var messages = await _queue.DequeueAsync(_options.BatchSize, stoppingToken);

                if (messages.Count > 0)
                {
                    _logger.LogDebug("Processing batch of {Count} messages.", messages.Count);

                    var processedIds = new List<string>(messages.Count);

                    foreach (var message in messages)
                    {
                        if (stoppingToken.IsCancellationRequested)
                            break;

                        bool success = await _processor.ProcessAsync(message, stoppingToken);

                        if (success)
                        {
                            processedIds.Add(message);
                            Interlocked.Increment(ref _processedCount);
                        }
                        else
                        {
                            Interlocked.Increment(ref _failedCount);
                            _logger.LogWarning("Processing failed for message: {MessageId}", message);
                        }
                    }

                    // Only acknowledge successfully processed messages
                    if (processedIds.Count > 0)
                    {
                        await _queue.AcknowledgeAsync(processedIds, stoppingToken);
                    }
                }

                // Wait for the next poll interval
                await Task.Delay(_options.PollInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected during shutdown
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal error in queue processing loop.");
                // Prevent tight loop on persistent failure
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        _logger.LogInformation("Queue Processor Service stopping.");
    }
}

// Health check implementation
public class QueueHealthCheck : IHealthCheck
{
    private readonly QueueProcessorService _processorService;

    public QueueHealthCheck(QueueProcessorService processorService)
    {
        _processorService = processorService ?? throw new ArgumentNullException(nameof(processorService));
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        int failures = _processorService.FailedCount;

        if (failures < 10)
        {
            return Task.FromResult(HealthCheckResult.Healthy($"Processing stable. Failures: {failures}"));
        }
        else
        {
            return Task.FromResult(HealthCheckResult.Unhealthy($"High failure rate detected. Failures: {failures}"));
        }
    }
}

// Extension methods for DI registration
public static class QueueProcessorServiceExtensions
{
    public static IServiceCollection AddQueueProcessor(this IServiceCollection services, Action<QueueProcessorOptions>? configure = null)
    {
        // Register options
        services.AddOptions<QueueProcessorOptions>()
                .Configure(options =>
                {
                    // Apply defaults are handled in the class, but we can enforce them here if needed
                    // or just run the user configuration.
                    configure?.Invoke(options);
                });

        // Register the background service
        // We register it as both BackgroundService and concrete type to allow direct resolution for HealthCheck
        services.AddSingleton<QueueProcessorService>();
        services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<QueueProcessorService>());

        // Register the health check
        services.AddHealthChecks()
                .AddCheck<QueueHealthCheck>("queue_processor_health_check");

        return services;
    }
}