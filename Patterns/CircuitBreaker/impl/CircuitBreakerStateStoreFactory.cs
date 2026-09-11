namespace BookTable.Patterns.CircuitBreaker.impl
{
    public class CircuitBreakerStateStoreFactory
    {
        // Return a singleton state store so the circuit breaker state is shared across
        // service instances and requests. Previously this returned a new instance on
        // every call which prevented the circuit from ever staying open.
        private static readonly ICircuitBreakerStateStore sharedInstance = new InMemoryCircuitBreakerStateStoreStateStoreStore();

        public static ICircuitBreakerStateStore GetCircuitBreakerStateStore()
        {
            return sharedInstance;
        }
    }
}
