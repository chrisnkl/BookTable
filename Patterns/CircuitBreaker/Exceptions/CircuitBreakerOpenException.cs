namespace BookTable.Patterns.CircuitBreaker
{
    public class CircuitBreakerOpenException : Exception
    {
        public CircuitBreakerOpenException()
            : base("The circuit breaker is open.")
        {
        }

        public CircuitBreakerOpenException(string message)
            : base(message)
        {
        }

        public CircuitBreakerOpenException(string message, Exception? innerException)
            : base(message, innerException)
        {
        }

        public CircuitBreakerOpenException(Exception? innerException)
            : base("The circuit breaker is open.", innerException)
        {
        }
    }
}
