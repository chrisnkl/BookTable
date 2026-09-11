namespace BookTable.Patterns.CircuitBreaker.impl
{
    public class CircuitBreaker
    {

        private readonly ICircuitBreakerStateStore stateStore = CircuitBreakerStateStoreFactory.GetCircuitBreakerStateStore();
        private readonly object halfOpenSyncObject = new object();
        private readonly TimeSpan OpenToHalfOpenWaitTime = new TimeSpan(0, 0, 60);

        public bool IsClosed { get { return stateStore.IsClosed; } }
        public bool IsOpen { get { return !IsClosed; } }

        public void ExecuteAction(Action action)
        {
            if (IsOpen)
            {
                if (stateStore.LastStateChangedDateUtc + OpenToHalfOpenWaitTime < DateTime.UtcNow)
                {
                    bool lockTaken = false;
                    try
                    {
                        Monitor.TryEnter(halfOpenSyncObject, ref lockTaken);
                        if (lockTaken)
                        {
                            // Set the circuit breaker state to HalfOpen.
                            stateStore.HalfOpen();
                            // Attempt the operation.
                            action();
                            // If this action succeeds, reset the state and allow other operations.
                            // In reality, instead of immediately returning to the Open state, a counter
                            // here would record the number of successful operations and return the
                            // circuit breaker to the Open state only after a specified number succeed.
                            this.stateStore.Reset();
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        // If there is still an exception, trip the breaker again immediately.
                        this.stateStore.Trip(ex);
                        // Throw the exception so that the caller knows which exception occurred.
                        throw;
                    }
                    finally
                    {
                        if (lockTaken)
                        {
                            Monitor.Exit(halfOpenSyncObject);
                        }
                    }
                }

                // The Open timeout has not yet expired. Throw a CircuitBreakerOpen exception to
                // inform the caller that the caller that the call was not actually attempted,
                // and return the most recent exception received.
                throw new CircuitBreakerOpenException(stateStore.LastException);
            }

            // The circuit breaker is Closed, execute the action.
            try
            {
                action();
            }
            catch (Exception ex)
            {
                // If an exception still occurs here, simply 
                // re-trip the breaker immediately.
                this.TrackException(ex);
                // Throw the exception so that the caller can tell
                // the type of exception that was thrown.
                throw;
            }
        }

        private void TrackException(Exception ex)
        {
            // For simplicity in this example, open the circuit breaker on the first exception.
            // In reality this would be more complex. A certain type of exception, such as one
            // that indicates a service is offline, might trip the circuit breaker immediately. 
            // Alternatively it may count exceptions locally or across multiple instances and
            // use this value over time, or the exception/success ratio based on the exception
            // types, to open the circuit breaker.
            this.stateStore.Trip(ex);
        }
    }
}
