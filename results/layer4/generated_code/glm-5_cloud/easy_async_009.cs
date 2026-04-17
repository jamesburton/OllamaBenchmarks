using System;
using System.Threading.Tasks;

public static class Retrier
{
    public static async Task<T> RetryAsync<T>(Func<Task<T>> operation, int maxAttempts)
    {
        if (operation == null)
        {
            throw new ArgumentNullException(nameof(operation));
        }

        if (maxAttempts < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxAttempts), "Max attempts must be at least 1.");
        }

        Exception lastException = null;

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                // Execute the operation
                return await operation().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                lastException = ex;

                // If this was the last attempt, do not wait, just proceed to throw
                if (attempt == maxAttempts)
                {
                    break;
                }
            }
        }

        // This line is only reached if all attempts failed
        throw lastException!;
    }
}