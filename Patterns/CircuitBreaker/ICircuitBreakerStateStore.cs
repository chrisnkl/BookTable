namespace BookTable.Patterns.CircuitBreaker
{
    public interface ICircuitBreakerStateStore
    {

        CircuitBreakerStateEnum State { get; }
        Exception? LastException { get; }
        DateTime LastStateChangedDateUtc { get; }
        void Trip(Exception e);
        void Reset();
        void HalfOpen();
        bool IsClosed { get; }

    }
}
