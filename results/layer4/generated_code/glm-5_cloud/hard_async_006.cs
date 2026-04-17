using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

public class AsyncThrottler
{
    private readonly int _maxConcurrent;
    private readonly TimeSpan _minInterval;
    private readonly SemaphoreSlim _concurrencySemaphore;
    private readonly Queue<long> _timestampHistory; // Tracks completion timestamps for sliding window
    private readonly object _rateLock = new object();

    public int PendingCount => _concurrencySemaphore.CurrentCount > 0 
        ? 0 
        : _maxConcurrent - _concurrencySemaphore.CurrentCount;

    public AsyncThrottler(int maxConcurrent, int maxPerSecond)
    {
        if (maxConcurrent <= 0) throw new ArgumentOutOfRangeException(nameof(maxConcurrent));
        if (maxPerSecond <= 0) throw new ArgumentOutOfRangeException(nameof(maxPerSecond));

        _maxConcurrent = maxConcurrent;
        _minInterval = TimeSpan.FromSeconds(1.0 / maxPerSecond);
        _concurrencySemaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
        _timestampHistory = new Queue<long>();
    }

    public async Task RunAsync(Func<CancellationToken, Task> work, CancellationToken ct)
    {
        await RunAsync(async token =>
        {
            await work(token).ConfigureAwait(false);
            return true;
        }, ct).ConfigureAwait(false);
    }

    public async Task<T> RunAsync<T>(Func<CancellationToken, Task<T>> work, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        // 1. Concurrency Limit: Wait for a slot
        await _concurrencySemaphore.WaitAsync(ct).ConfigureAwait(false);

        try
        {
            // 2. Rate Limit: Sliding Window Algorithm
            await EnforceRateLimitAsync(ct).ConfigureAwait(false);

            // 3. Execute Work
            return await work(ct).ConfigureAwait(false);
        }
        finally
        {
            // 4. Release Concurrency Slot
            _concurrencySemaphore.Release();
        }
    }

    private async Task EnforceRateLimitAsync(CancellationToken ct)
    {
        while (true)
        {
            ct.ThrowIfCancellationRequested();

            long nowTicks = Stopwatch.GetTimestamp();
            double secondsPerTick = 1.0 / Stopwatch.Frequency;
            long windowTicks = (long)(_minInterval.TotalSeconds * Stopwatch.Frequency);

            lock (_rateLock)
            {
                // Remove timestamps older than the minimum interval
                while (_timestampHistory.Count > 0)
                {
                    long timestamp = _timestampHistory.Peek();
                    double elapsedSeconds = (nowTicks - timestamp) * secondsPerTick;

                    if (elapsedSeconds >= _minInterval.TotalSeconds)
                    {
                        _timestampHistory.Dequeue();
                    }
                    else
                    {
                        break;
                    }
                }

                // If we haven't hit the rate implied by the window (conceptually 1 request per interval),
                // allow the request.
                // Note: We check if the queue is empty or if the oldest entry is outside the window.
                // Since we clean above, if Count > 0, we are inside the window.
                if (_timestampHistory.Count == 0)
                {
                    _timestampHistory.Enqueue(nowTicks);
                    return;
                }
            }

            // Calculate delay needed for the oldest request to age out
            long oldestTick;
            lock (_rateLock)
            {
                if (_timestampHistory.TryPeek(out oldestTick))
                {
                    // Continue processing
                }
                else
                {
                    // Should not happen if Count > 0, but safe fallback
                    continue; 
                }
            }

            long elapsedSinceOldest = nowTicks - oldestTick;
            long remainingTicks = windowTicks - elapsedSinceOldest;

            if (remainingTicks <= 0)
            {
                // Time has passed, loop again to re-check and clean queue
                continue;
            }

            // Wait for the remainder of the interval
            double delaySeconds = remainingTicks * secondsPerTick;
            TimeSpan delay = TimeSpan.FromSeconds(delaySeconds);

            // Add a small buffer to prevent tight loop spinning due to timer resolution
            TimeSpan delayWithBuffer = delay.Add(TimeSpan.FromMilliseconds(1));

            await Task.Delay(delayWithBuffer, ct).ConfigureAwait(false);
        }
    }
}