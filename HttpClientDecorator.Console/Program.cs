using Polly;
using Polly.CircuitBreaker;
using System.Security.Cryptography;

const int CircuitBreakerThreshold = 1;
const int CircuitBreakerDurationInMS = 1000;
const int MaxConcurrentRequests = 1;
const int NumberOfRequests = 100;


Console.WriteLine("Hello, World!");
var results = await DoWork(
    CircuitBreakerThreshold,
    CircuitBreakerDurationInMS,
    MaxConcurrentRequests,
    NumberOfRequests,
    new HttpClient());

foreach (var (Iteration, ResponseContent) in results)
{
    Console.WriteLine($"{Iteration}:{ResponseContent}");
}
Console.WriteLine("We are done!");
Console.ReadKey();


void OnCircuitBreakerOpened(Exception exception, TimeSpan span)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Circuit breaker opened. No requests will be made for the specified timespan.\n\t{exception.Message}");
    Console.ResetColor();
}

void OnCircuitBreakerReset()
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Circuit breaker reset. Requests will be attempted again.");
    Console.ResetColor();
}

void OnCircuitBreakerHalfOpen()
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("Circuit breaker half-open. Testing if requests can be made again.");
    Console.ResetColor();
}

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
        , onBreak: OnCircuitBreakerOpened
        , onReset: OnCircuitBreakerReset
        , onHalfOpen: OnCircuitBreakerHalfOpen);
    var semaphore = new SemaphoreSlim(maxConcurrentRequests);
    var tasks = new List<Task>();
    var results = new List<(int Iteration, string ResponseContent)>(); // List to store the results


    for (int i = 0; i < numberOfRequests; i++)
    {
        try // Catch Exceptions 
        {
            int iterationNumber = i;
            await semaphore.WaitAsync(); // Acquire a semaphore slot
            tasks.Add(Task.Run(async () =>
            {
                try // Try Block for Catching Broken Circuit Finally release Semaphore
                {
                    await circuitBreakerPolicy.ExecuteAsync(async () =>
                    {
                        int relativeWorkRequested = RandomNumberGenerator.GetInt32(2, 5);
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
