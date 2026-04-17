using System;
using System.Threading;
using System.Threading.Tasks;

public class DeadlineCancellationService
{
    /// <summary>
    /// Creates a linked CancellationTokenSource that cancels after the specified deadline 
    /// or when the caller's token is cancelled.
    /// </summary>
    public CancellationTokenSource CreateDeadlineSource(TimeSpan deadline, CancellationToken callerCt)
    {
        var deadlineCts = new CancellationTokenSource();

        // If the caller token is already cancelled, we can propagate immediately.
        if (callerCt.IsCancellationRequested)
        {
            deadlineCts.Cancel();
        }
        else
        {
            // Link the caller token to the deadline source.
            // Using Register allows us to cancel the deadline source when the caller cancels.
            callerCt.Register(state => 
            {
                var cts = state as CancellationTokenSource;
                cts?.Cancel();
            }, deadlineCts);
        }

        // Schedule automatic cancellation after the deadline.
        deadlineCts.CancelAfter(deadline);

        return deadlineCts;
    }

    /// <summary>
    /// Runs work with a combined token. 
    /// On timeout, wraps OperationCanceledException in TimeoutException.
    /// If caller cancelled, rethrows OperationCanceledException as-is.
    /// </summary>
    public async Task<T> RunWithDeadlineAsync<T>(Func<CancellationToken, Task<T>> work, TimeSpan deadline, CancellationToken callerCt)
    {
        // Create a linked source that cancels on deadline or caller cancellation
        using var linkedCts = CreateDeadlineSource(deadline, callerCt);

        try
        {
            return await work(linkedCts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException ex)
        {
            // If the caller token triggered the cancellation, rethrow as-is.
            if (callerCt.IsCancellationRequested)
            {
                throw;
            }

            // If the deadline token triggered the cancellation, wrap in TimeoutException.
            // Note: We check linkedCts.IsCancellationRequested to ensure it was our deadline source.
            if (linkedCts.IsCancellationRequested && !callerCt.IsCancellationRequested)
            {
                throw new TimeoutException("Deadline exceeded", ex);
            }

            // Otherwise, rethrow the original exception (could be from work logic passing a different token).
            throw;
        }
    }

    /// <summary>
    /// Returns result of work if it completes within deadline, otherwise returns fallback result.
    /// </summary>
    public async Task<T> RunWithFallbackAsync<T>(Func<CancellationToken, Task<T>> work, Func<Task<T>> fallback, TimeSpan deadline, CancellationToken callerCt)
    {
        try
        {
            // Attempt to run the work with the deadline constraint.
            return await RunWithDeadlineAsync(work, deadline, callerCt).ConfigureAwait(false);
        }
        catch (TimeoutException)
        {
            // If a timeout occurs, execute the fallback.
            // We assume the fallback does not respect the original deadline or caller token 
            // unless specified, but we execute it safely.
            return await fallback().ConfigureAwait(false);
        }
        // Note: If callerCt cancels, RunWithDeadlineAsync throws OperationCanceledException, 
        // which propagates out of this method (no fallback).
    }
}