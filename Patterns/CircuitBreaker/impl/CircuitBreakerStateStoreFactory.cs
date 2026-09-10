namespace BookTable.Patterns.CircuitBreaker.impl
{
    public class CircuitBreakerStateStoreFactory
    {
        // Return a singleton state store so the circuit breaker state is shared across
        // service instances and requests. Previously this returned a new instance on
        // every call which prevented the circuit from ever staying open.
        private static readonly ICircuitBreaker sharedInstance = new InMemoryCircuitBreakerStateStore();

        public static ICircuitBreaker GetCircuitBreakerStateStore()
        {
            return sharedInstance;
        }
    }
}
