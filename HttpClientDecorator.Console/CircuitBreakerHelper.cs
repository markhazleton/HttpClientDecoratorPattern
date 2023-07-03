namespace HttpClientDecorator.Helpers;

public static class CircuitBreakerHelper
{
    public static void OnCircuitBreakerOpened(Exception exception, TimeSpan span)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Circuit breaker opened. No requests will be made for the specified timespan.\n\t{exception.Message}");
        Console.ResetColor();
    }

    public static void OnCircuitBreakerReset()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Circuit breaker reset. Requests will be attempted again.");
        Console.ResetColor();
    }

    public static void OnCircuitBreakerHalfOpen()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Circuit breaker half-open. Testing if requests can be made again.");
        Console.ResetColor();
    }



}
