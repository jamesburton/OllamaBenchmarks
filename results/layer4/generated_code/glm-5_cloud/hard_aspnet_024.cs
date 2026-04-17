using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Register the service
        builder.Services.AddScoped<IDataStreamService, SimulatedDataStreamService>();

        var app = builder.Build();

        // Map endpoints using the extension method
        app.MapStreamEndpoints();

        app.Run();
    }
}

// --- Models ---

public record DataPoint(int Index, double Value, DateTime Timestamp);

// --- Service Interface & Implementation ---

public interface IDataStreamService
{
    IAsyncEnumerable<DataPoint> StreamAsync(int count, TimeSpan interval, CancellationToken ct);
}

public class SimulatedDataStreamService : IDataStreamService
{
    private static readonly Random _random = new Random();

    public async IAsyncEnumerable<DataPoint> StreamAsync(int count, TimeSpan interval, [EnumeratorCancellation] CancellationToken ct)
    {
        for (int i = 0; i < count; i++)
        {
            ct.ThrowIfCancellationRequested();

            // Generate random data
            double value = _random.NextDouble() * 100.0;
            var point = new DataPoint(i, value, DateTime.UtcNow);

            // Simulate work/wait time
            if (interval > TimeSpan.Zero)
            {
                await Task.Delay(interval, ct);
            }

            yield return point;
        }
    }
}

// --- Endpoints ---

public static class StreamEndpoints
{
    public static void MapStreamEndpoints(this WebApplication app)
    {
        // GET /stream?count=100&intervalMs=10 — streams DataPoints as NDJSON
        app.MapGet("/stream", async (IDataStreamService service, int? count, int? intervalMs, CancellationToken ct) =>
        {
            // Validation
            if (!count.HasValue || count.Value < 1 || count.Value > 10000)
            {
                return Results.BadRequest("Count must be between 1 and 10000.");
            }

            if (!intervalMs.HasValue || intervalMs.Value < 0 || intervalMs.Value > 5000)
            {
                return Results.BadRequest("IntervalMs must be between 0 and 5000.");
            }

            var interval = TimeSpan.FromMilliseconds(intervalMs.Value);

            // Stream response as NDJSON
            return Results.Stream(async (responseStream, cancellationToken) =>
            {
                await using var writer = new StreamWriter(responseStream, leaveOpen: true);

                await foreach (var point in service.StreamAsync(count.Value, interval, cancellationToken))
                {
                    // Serialize to JSON and add newline
                    await writer.WriteLineAsync(System.Text.Json.JsonSerializer.Serialize(point));
                    await writer.FlushAsync(cancellationToken);
                }
            }, "application/x-ndjson");

        });

        // GET /stream/batch?count=50 — returns all points as a JSON array
        app.MapGet("/stream/batch", async (IDataStreamService service, int? count) =>
        {
            // Validation
            if (!count.HasValue || count.Value < 1 || count.Value > 10000)
            {
                return Results.BadRequest("Count must be between 1 and 10000.");
            }

            // Collect stream to list
            var points = new List<DataPoint>();
            await foreach (var point in service.StreamAsync(count.Value, TimeSpan.Zero, CancellationToken.None))
            {
                points.Add(point);
            }

            return Results.Ok(points);
        });
    }
}

// --- Statistics ---

public static class DataPointStatistics
{
    public static (double Min, double Max, double Avg, double StdDev) Compute(IReadOnlyList<DataPoint> points)
    {
        if (points == null || points.Count == 0)
        {
            throw new ArgumentException("Points collection cannot be null or empty.", nameof(points));
        }

        var values = points.Select(p => p.Value).ToList();

        double min = values.Min();
        double max = values.Max();
        double avg = values.Average();

        double sumOfSquaresOfDifferences = values.Sum(val => Math.Pow(val - avg, 2.0));
        double stdDev = Math.Sqrt(sumOfSquaresOfDifferences / values.Count);

        return (min, max, avg, stdDev);
    }
}