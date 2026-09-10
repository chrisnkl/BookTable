namespace BookTable.Patterns.CircuitBreaker.impl
{
    public class InMemoryCircuitBreakerStateStore : ICircuitBreaker
    {
        public CircuitBreakerStateEnum State { get; private set; } = CircuitBreakerStateEnum.Closed;
        public Exception? LastException { get; private set; }
        public DateTime LastStateChangedDateUtc { get; private set; } = DateTime.UtcNow;

        public bool IsClosed => State == CircuitBreakerStateEnum.Closed;

        public void Trip(Exception e)
        {
            State = CircuitBreakerStateEnum.Open;
            LastException = e;
            LastStateChangedDateUtc = DateTime.UtcNow;
        }

        public void Reset()
        {
            State = CircuitBreakerStateEnum.Closed;
            LastException = null;
            LastStateChangedDateUtc = DateTime.UtcNow;
        }

        public void HalfOpen()
        {
            State = CircuitBreakerStateEnum.HalfOpen;
            LastStateChangedDateUtc = DateTime.UtcNow;
        }
    }
}
