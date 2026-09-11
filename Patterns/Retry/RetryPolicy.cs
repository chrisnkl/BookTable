using System.Diagnostics;
using System.Net;
using BookTable.Patterns.CircuitBreaker;
using Microsoft.Data.SqlClient;

namespace BookTable.Patterns.Retry
{
    public class RetryPolicy
    {
        public readonly int retryCount;
        private readonly TimeSpan initialDelay;
        private readonly bool useExponentialBackoff;

        public RetryPolicy(int retryCount = 3, TimeSpan? initialDelay = null, bool useExponentialBackoff = true)
        {
            this.retryCount = retryCount;
            this.initialDelay = initialDelay ?? TimeSpan.FromMilliseconds(100);
            this.useExponentialBackoff = useExponentialBackoff;
        }

        public async Task ExecuteAsync(Func<Task> operation)
        {
            int currentRetry = 0;
            for (; ;)
            {
                try
                {
                    await operation();
                    break;
                }
                catch (Exception ex)
                {
                    Trace.TraceError("Operation Exception in RetryPolicy: {0}", ex.Message);
                    currentRetry++;

                    if (currentRetry > this.retryCount || !IsTransient(ex))
                    {
                        throw;
                    }
                }

                TimeSpan delay = CalculateDelay(currentRetry);
                await Task.Delay(delay);
            }
        }

        public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
        {
            int currentRetry = 0;
            for (; ;)
            {
                try
                {
                    return await operation();
                }
                catch (Exception ex)
                {
                    Trace.TraceError("Operation Exception in RetryPolicy: {0}", ex.Message);
                    currentRetry++;

                    if (currentRetry > this.retryCount || !IsTransient(ex))
                    {
                        throw;
                    }
                }

                TimeSpan delay = CalculateDelay(currentRetry);
                await Task.Delay(delay);
            }
        }

        public void Execute(Action operation)
        {
            int currentRetry = 0;
            for (; ;)
            {
                try
                {
                    operation();
                    break;
                }
                catch (Exception ex)
                {
                    Trace.TraceError("Operation Exception in RetryPolicy: {0}", ex.Message);
                    currentRetry++;

                    if (currentRetry > this.retryCount || !IsTransient(ex))
                    {
                        throw;
                    }
                }

                TimeSpan delay = CalculateDelay(currentRetry);
                Thread.Sleep(delay);
            }
        }

        public T Execute<T>(Func<T> operation)
        {
            int currentRetry = 0;
            for (; ;)
            {
                try
                {
                    return operation();
                }
                catch (Exception ex)
                {
                    Trace.TraceError("Operation Exception in RetryPolicy: {0}", ex.Message);
                    currentRetry++;

                    if (currentRetry > this.retryCount || !IsTransient(ex))
                    {
                        throw;
                    }
                }

                TimeSpan delay = CalculateDelay(currentRetry);
                Thread.Sleep(delay);
            }
        }

        private TimeSpan CalculateDelay(int retryAttempt)
        {
            if (!useExponentialBackoff)
            {
                return initialDelay;
            }

            double multiplier = Math.Pow(2, retryAttempt - 1);
            return TimeSpan.FromMilliseconds(initialDelay.TotalMilliseconds * multiplier);
        }

        public bool IsTransient(Exception ex)
        {
            // As detailed in the Microsoft Cloud Design Patterns book:
            // The retry logic should be sensitive to any exceptions returned by the circuit breaker
            // and abandon retry attempts if the circuit breaker indicates that a fault is not transient.
            if (ex is CircuitBreakerOpenException)
            {
                return false;
            }

            if (ex is TimeoutException || ex is OperationCanceledException)
            {
                return true;
            }

            if (ex is WebException webException)
            {
                return new[]
                {
                    WebExceptionStatus.ConnectionClosed,
                    WebExceptionStatus.Timeout,
                    WebExceptionStatus.RequestCanceled,
                    WebExceptionStatus.ConnectFailure,
                    WebExceptionStatus.NameResolutionFailure
                }.Contains(webException.Status);
            }

            if (ex is HttpRequestException)
            {
                return true;
            }

            if (ex.InnerException != null && IsTransient(ex.InnerException))
            {
                return true;
            }

            return false;
        }
    }
}
