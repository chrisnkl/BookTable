namespace BookTable.Patterns.CircuitBreaker.impl
{
    public class CircuitBreakerStateStoreFactory
    {
        public static ICircuitBreaker GetCircuitBreakerStateStore()
        {
            return new InMemoryCircuitBreakerStateStore();
        }
    }
}
