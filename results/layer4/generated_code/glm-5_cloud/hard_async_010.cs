using System;
using System.Threading;
using System.Threading.Tasks;

namespace CircuitBreakerPattern
{
    /// <summary>
    /// Defines the possible states of the circuit breaker.
    /// </summary>
    public enum CircuitState
    {
        Closed,
        Open,
        HalfOpen
    }

    /// <summary>
    /// The exception that is thrown when the circuit breaker is in the Open state 
    /// and an operation is attempted.
    /// </summary>
    public class CircuitBreakerOpenException : Exception
    {
        public CircuitBreakerOpenException() : base("The circuit breaker is currently open and cannot execute operations.") { }
        public CircuitBreakerOpenException(string message) : base(message) { }
        public CircuitBreakerOpenException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// An asynchronous implementation of the Circuit Breaker pattern.
    /// </summary>
    /// <typeparam name="T">The return type of the operations executed by the circuit breaker.</typeparam>
    public class AsyncCircuitBreaker<T>
    {
        private readonly int _failureThreshold;
        private readonly TimeSpan _openDuration;

        private readonly object _lock = new object();
        private CircuitState _state = CircuitState.Closed;
        private int _failureCount = 0;
        private DateTime _lastFailureTime;

        /// <summary>
        /// Gets the current state of the circuit breaker.
        /// </summary>
        public CircuitState State
        {
            get
            {
                lock (_lock)
                {
                    // Check for automatic transition from Open to HalfOpen
                    if (_state == CircuitState.Open && (DateTime.UtcNow - _lastFailureTime) >= _openDuration)
                    {
                        _state = CircuitState.HalfOpen;
                    }
                    return _state;
                }
            }
        }

        /// <summary>
        /// Gets the current consecutive failure count.
        /// </summary>
        public int FailureCount
        {
            get
            {
                lock (_lock)
                {
                    return _failureCount;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncCircuitBreaker{T}"/> class.
        /// </summary>
        /// <param name="failureThreshold">The number of consecutive failures required to transition from Closed to Open.</param>
        /// <param name="openDuration">The duration the circuit breaker stays open before transitioning to HalfOpen.</param>
        public AsyncCircuitBreaker(int failureThreshold, TimeSpan openDuration)
        {
            if (failureThreshold <= 0)
                throw new ArgumentOutOfRangeException(nameof(failureThreshold), "Failure threshold must be greater than zero.");
            if (openDuration.TotalMilliseconds <= 0)
                throw new ArgumentOutOfRangeException(nameof(openDuration), "Open duration must be greater than zero.");

            _failureThreshold = failureThreshold;
            _openDuration = openDuration;
        }

        /// <summary>
        /// Executes the specified asynchronous operation if the circuit breaker allows it.
        /// </summary>
        /// <param name="operation">The asynchronous operation to execute.</param>
        /// <param name="ct">A cancellation token.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of type T.</returns>
        /// <exception cref="CircuitBreakerOpenException">Thrown when the circuit breaker is Open.</exception>
        public async Task<T> ExecuteAsync(Func<CancellationToken, Task<T>> operation, CancellationToken ct)
        {
            if (operation == null) throw new ArgumentNullException(nameof(operation));

            // Check state and determine if execution is allowed
            bool isHalfOpenCall = false;

            lock (_lock)
            {
                var currentState = GetStateInternal();

                switch (currentState)
                {
                    case CircuitState.Open:
                        throw new CircuitBreakerOpenException();

                    case CircuitState.HalfOpen:
                        // Allow one test call
                        isHalfOpenCall = true;
                        break;

                    case CircuitState.Closed:
                        // Normal operation
                        break;
                }
            }

            try
            {
                T result = await operation(ct).ConfigureAwait(false);

                // Success
                lock (_lock)
                {
                    OnSuccess(isHalfOpenCall);
                }

                return result;
            }
            catch (OperationCanceledException)
            {
                // Cancellation is not considered a failure for the circuit breaker logic
                throw;
            }
            catch (Exception)
            {
                // Failure
                lock (_lock)
                {
                    OnFailure(isHalfOpenCall);
                }
                throw;
            }
        }

        /// <summary>
        /// Helper to get state and auto-transition Open -> HalfOpen if time has elapsed.
        /// Must be called within a lock.
        /// </summary>
        private CircuitState GetStateInternal()
        {
            if (_state == CircuitState.Open && (DateTime.UtcNow - _lastFailureTime) >= _openDuration)
            {
                _state = CircuitState.HalfOpen;
            }
            return _state;
        }

        /// <summary>
        /// Handles logic when an operation succeeds.
        /// Must be called within a lock.
        /// </summary>
        private void OnSuccess(bool wasHalfOpenCall)
        {
            if (wasHalfOpenCall || _state == CircuitState.HalfOpen)
            {
                // If we were in HalfOpen, reset to Closed
                Reset();
            }
            else if (_state == CircuitState.Closed)
            {
                // If we were Closed, reset failure count on success
                _failureCount = 0;
            }
        }

        /// <summary>
        /// Handles logic when an operation fails.
        /// Must be called within a lock.
        /// </summary>
        private void OnFailure(bool wasHalfOpenCall)
        {
            if (wasHalfOpenCall || _state == CircuitState.HalfOpen)
            {
                // Failure in HalfOpen -> Open immediately
                Trip();
            }
            else if (_state == CircuitState.Closed)
            {
                _failureCount++;
                if (_failureCount >= _failureThreshold)
                {
                    Trip();
                }
            }
        }

        /// <summary>
        /// Transitions the circuit breaker to the Open state.
        /// Must be called within a lock.
        /// </summary>
        private void Trip()
        {
            _state = CircuitState.Open;
            _lastFailureTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Resets the circuit breaker to the Closed state.
        /// Must be called within a lock.
        /// </summary>
        private void Reset()
        {
            _state = CircuitState.Closed;
            _failureCount = 0;
        }
    }
}