using HttpClientDecorator.Helpers;
using Polly;
using Polly.CircuitBreaker;
using System.Security.Cryptography;

const int CircuitBreakerThreshold = 3;
const int CircuitBreakerDurationInMS = 1000;
const int MaxConcurrentRequests = 1;
const int NumberOfRequests = 100;

Console.WriteLine("Hello, World!");
//var results = await DoWork(
//    CircuitBreakerThreshold,
//    CircuitBreakerDurationInMS,
//    MaxConcurrentRequests,
//    NumberOfRequests,
//    new HttpClient());

//foreach (var (Iteration, ResponseContent) in results)
//{
//    Console.WriteLine($"{Iteration}:{ResponseContent}");
//}
//Console.WriteLine("We are done!");
//Console.ReadKey();


// Configure the circuit breaker policy
var circuitBreakerPolicy = Policy
    .Handle<HttpRequestException>()
    .CircuitBreaker(
        exceptionsAllowedBeforeBreaking: 3,
        durationOfBreak: TimeSpan.FromSeconds(30),
        onBreak: (ex, breakDelay) =>
        {
            Console.WriteLine($"Circuit breaker opened. Delaying for {breakDelay.TotalSeconds} seconds.");
        },
        onReset: () =>
        {
            Console.WriteLine("Circuit breaker reset.");
        },
        onHalfOpen: () =>
        {
            Console.WriteLine("Circuit breaker half-opened.");
        }
    );

// Execute an HTTP request with the circuit breaker policy
circuitBreakerPolicy.Execute(() =>
{
    try
    {
        using (var httpClient = new HttpClient())
        {
            var response = httpClient.GetAsync("https://asyncdemo.controlorigins.com/status").Result;
            response.EnsureSuccessStatusCode();

            var result = response.Content.ReadAsStringAsync().Result;
            Console.WriteLine($"Response: {result}");
        }
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
        throw;
    }
});
async Task<List<(int Iteration, string ResponseContent)>> DoWork(
    int circuitBreakerThreshold,
    double circuitBreakerTimeSpan,
    int maxConcurrentRequests,
    int numberOfRequests,
    HttpClient client)
{
    var circuitBreakerPolicy = Policy.Handle<Exception>()
        .CircuitBreakerAsync(circuitBreakerThreshold
        , TimeSpan.FromMicroseconds(circuitBreakerTimeSpan)
        , onBreak: CircuitBreakerHelper.OnCircuitBreakerOpened
        , onReset: CircuitBreakerHelper.OnCircuitBreakerReset
        , onHalfOpen: CircuitBreakerHelper.OnCircuitBreakerHalfOpen);
    var semaphore = new SemaphoreSlim(maxConcurrentRequests);
    var tasks = new List<Task>();
    var results = new List<(int Iteration, string ResponseContent)>(); // List to store the results


    for (int i = 0; i < numberOfRequests; i++)
    {
        try // Catch Exceptions 
        {
            int iterationNumber = i;
            await semaphore.WaitAsync(); // Acquire a semaphore slot

            // await Task.Delay(200); // Delay in milliseconds

            tasks.Add(Task.Run(async () =>
            {
                try // Try Block for Catching Broken Circuit Finally release Semaphore
                {
                    await circuitBreakerPolicy.ExecuteAsync(async () =>
                    {
                        int relativeWorkRequested = RandomNumberGenerator.GetInt32(20, 25);
                        try // Try Block for Capturing Request Exceptions
                        {
                            var request = new HttpRequestMessage(HttpMethod.Post, "https://asyncdemoweb.azurewebsites.net/api/remote/Results");
                            var content = new StringContent("{\r\n  \"loopCount\": " + relativeWorkRequested + ",\r\n  \"maxTimeMS\": 100,\r\n  \"runTimeMS\": 0,\r\n  \"message\": \"string\",\r\n  \"resultValue\": \"string\"\r\n}", null, "application/json");
                            request.Content = content;
                            var response = await client.SendAsync(request);
                            response.EnsureSuccessStatusCode();
                            var ResponseContent = await response.Content.ReadAsStringAsync();
                            results.Add((iterationNumber, ResponseContent));
                        }
                        catch (Exception ex)
                        {
                            results.Add((iterationNumber, ex.Message));
                        }
                    });
                }
                catch (BrokenCircuitException)
                {
                    Console.WriteLine("BrokenCircuitException: Circuit breaker opened. Requests are being blocked.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"CircuitTryException: An error occurred: {ex.Message}");
                }
                finally
                {
                    semaphore.Release(); // Release the semaphore slot
                }
            }));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SemaphoreTry:An error occurred: {ex.Message}");
        }
    }
    // Wait for all tasks to complete
    await Task.WhenAll(tasks);
    return results;
}
