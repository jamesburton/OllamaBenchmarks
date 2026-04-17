using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class FanOutAggregator
{
    /// <summary>
    /// Executes all operations concurrently, aggregating successes and failures.
    /// Does not throw exceptions for individual operation failures; returns them in the result.
    /// </summary>
    public async Task<AggregateResult<T>> FanOutAsync<T>(
        IEnumerable<Func<CancellationToken, Task<T>>> operations,
        CancellationToken ct)
    {
        var stopwatch = Stopwatch.StartNew();

        // Materialize the list to avoid multiple enumeration
        var operationList = operations.ToList();

        if (operationList.Count == 0)
        {
            stopwatch.Stop();
            return new AggregateResult<T>(Array.Empty<T>(), Array.Empty<Exception>(), stopwatch.Elapsed);
        }

        // Create a wrapper task that catches exceptions for each operation
        var tasks = operationList.Select(op => SafeExecuteAsync(op, ct));

        // Wait for all tasks to complete (either success or failure)
        var results = await Task.WhenAll(tasks).ConfigureAwait(false);

        stopwatch.Stop();

        var succeeded = results
            .Where(r => r.IsSuccess)
            .Select(r => r.Value!)
            .ToList();

        var failed = results
            .Where(r => !r.IsSuccess)
            .Select(r => r.Exception!)
            .ToList();

        return new AggregateResult<T>(succeeded, failed, stopwatch.Elapsed);
    }

    /// <summary>
    /// Executes all operations concurrently. 
    /// Throws AggregateException if any operation fails.
    /// </summary>
    public async Task<T[]> FanOutOrThrowAsync<T>(
        IEnumerable<Func<CancellationToken, Task<T>>> operations,
        CancellationToken ct)
    {
        var operationList = operations.ToList();

        if (operationList.Count == 0)
        {
            return Array.Empty<T>();
        }

        var tasks = operationList.Select(op => op(ct));
        var results = await Task.WhenAll(tasks).ConfigureAwait(false);

        return results;
    }

    /// <summary>
    /// Executes all operations concurrently with a global timeout.
    /// Throws TimeoutException if the operations do not complete within the specified timeout.
    /// Throws AggregateException if any operation fails before the timeout.
    /// </summary>
    public async Task<T[]> FanOutWithTimeoutAsync<T>(
        IEnumerable<Func<CancellationToken, Task<T>>> operations,
        TimeSpan timeout,
        CancellationToken ct)
    {
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        linkedCts.CancelAfter(timeout);

        var operationList = operations.ToList();

        if (operationList.Count == 0)
        {
            return Array.Empty<T>();
        }

        try
        {
            var tasks = operationList.Select(op => op(linkedCts.Token));
            var results = await Task.WhenAll(tasks).ConfigureAwait(false);
            return results;
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            // This block executes if the cancellation came from the timeout (linkedCts), 
            // not the external token (ct).
            throw new TimeoutException($"The operation timed out after {timeout.TotalMilliseconds}ms.");
        }
    }

    // Helper struct to safely capture results without throwing
    private async Task<OperationResult<T>> SafeExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken ct)
    {
        try
        {
            var result = await operation(ct).ConfigureAwait(false);
            return new OperationResult<T>(true, result, null);
        }
        catch (Exception ex)
        {
            return new OperationResult<T>(false, default, ex);
        }
    }

    private readonly struct OperationResult<T>
    {
        public bool IsSuccess { get; }
        public T? Value { get; }
        public Exception? Exception { get; }

        public OperationResult(bool isSuccess, T? value, Exception? exception)
        {
            IsSuccess = isSuccess;
            Value = value;
            Exception = exception;
        }
    }
}

public record AggregateResult<T>(
    IReadOnlyList<T> Succeeded,
    IReadOnlyList<Exception> Failed,
    TimeSpan Duration
);