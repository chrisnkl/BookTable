namespace BookTable.Patterns.CircuitBreaker.impl
{
    public class CircuitBreakerStateStoreFactory
    {
        // Return a singleton state store so the circuit breaker state is shared across service instances and requests.
        private static readonly ICircuitBreakerStateStore sharedInstance = new InMemoryCircuitBreakerStateStoreStateStoreStore();

        public static ICircuitBreakerStateStore GetCircuitBreakerStateStore()
        {
            return sharedInstance;
        }
    }
}
