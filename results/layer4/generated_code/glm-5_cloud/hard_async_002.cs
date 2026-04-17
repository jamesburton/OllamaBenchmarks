using System;
using System.Threading;
using System.Threading.Tasks;

namespace RetryPolicies
{
    /// <summary>
    /// Provides retry functionality with exponential backoff.
    /// </summary>
    public class AsyncRetryPolicy
    {
        /// <summary>
        /// Executes an async operation with retry and exponential backoff.
        /// </summary>
        /// <typeparam name="T">The return type of the operation.</typeparam>
        /// <param name="operation">The async operation to execute.</param>
        /// <param name="maxAttempts">Maximum number of attempts.</param>
        /// <param name="initialDelay">Initial delay for backoff.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The result of the operation.</returns>
        public async Task<T> ExecuteAsync<T>(
            Func<CancellationToken, Task<T>> operation,
            int maxAttempts,
            TimeSpan initialDelay,
            CancellationToken ct)
        {
            return await ExecuteAsync(operation, maxAttempts, initialDelay, ct, null).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes an async operation with retry, exponential backoff, and an optional retry callback.
        /// </summary>
        /// <typeparam name="T">The return type of the operation.</typeparam>
        /// <param name="operation">The async operation to execute.</param>
        /// <param name="maxAttempts">Maximum number of attempts.</param>
        /// <param name="initialDelay">Initial delay for backoff.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <param name="onRetry">Optional callback invoked on retry.</param>
        /// <returns>The result of the operation.</returns>
        public async Task<T> ExecuteAsync<T>(
            Func<CancellationToken, Task<T>> operation,
            int maxAttempts,
            TimeSpan initialDelay,
            CancellationToken ct,
            Func<RetryContext, Task>? onRetry)
        {
            if (operation == null) throw new ArgumentNullException(nameof(operation));
            if (maxAttempts < 1) throw new ArgumentOutOfRangeException(nameof(maxAttempts), "Max attempts must be at least 1.");
            if (initialDelay < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(initialDelay), "Initial delay must be non-negative.");

            // Do not retry if already cancelled
            if (ct.IsCancellationRequested)
            {
                ct.ThrowIfCancellationRequested();
            }

            int attempt = 0;
            Exception? lastException = null;

            while (true)
            {
                attempt++;
                try
                {
                    return await operation(ct).ConfigureAwait(false);
                }
                catch (Exception ex) when (ct.IsCancellationRequested)
                {
                    // If cancellation was requested during the operation, propagate immediately
                    throw;
                }
                catch (Exception ex)
                {
                    lastException = ex;

                    if (attempt >= maxAttempts)
                    {
                        break;
                    }

                    // Calculate exponential backoff delay
                    TimeSpan delay = TimeSpan.FromTicks(initialDelay.Ticks * (long)Math.Pow(2, attempt - 1));

                    // Invoke retry callback if provided
                    if (onRetry != null)
                    {
                        var context = new RetryContext(attempt, ex);
                        await onRetry(context).ConfigureAwait(false);
                    }

                    // Wait for the delay or until cancellation is requested
                    await Task.Delay(delay, ct).ConfigureAwait(false);
                }
            }

            // All attempts failed
            throw lastException!;
        }

        /// <summary>
        /// Executes an async operation with retry and exponential backoff (void overload).
        /// </summary>
        /// <param name="operation">The async operation to execute.</param>
        /// <param name="maxAttempts">Maximum number of attempts.</param>
        /// <param name="initialDelay">Initial delay for backoff.</param>
        /// <param name="ct">Cancellation token.</param>
        public async Task ExecuteAsync(
            Func<CancellationToken, Task> operation,
            int maxAttempts,
            TimeSpan initialDelay,
            CancellationToken ct)
        {
            // Delegate to the typed overload with a dummy wrapper
            await ExecuteAsync<bool>(async token =>
            {
                await operation(token).ConfigureAwait(false);
                return true;
            }, maxAttempts, initialDelay, ct).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Context passed to the onRetry callback containing attempt details.
    /// </summary>
    /// <param name="Attempt">The current attempt number (1-based).</param>
    /// <param name="LastException">The exception that caused the retry.</param>
    public record RetryContext(int Attempt, Exception LastException);
}