using Polly;
using Polly.CircuitBreaker;
using System.Security.Cryptography;

const int CircuitBreakerThreshold = 1;
const int CircuitBreakerDurationInMS = 100;
const int MaxConcurrentRequests = 1;
const int NumberOfRequests = 200;
int loopCount = 10;

void OnCircuitBreakerOpened(Exception exception, TimeSpan span)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Circuit breaker opened. No requests will be made for the specified timespan.\n\t{exception.Message}");
    loopCount = 1;
    Console.ResetColor();
}

void OnCircuitBreakerReset()
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Circuit breaker reset. Requests will be attempted again.");
    loopCount = 1;
    Console.ResetColor();
}

void OnCircuitBreakerHalfOpen()
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("Circuit breaker half-open. Testing if requests can be made again.");
    Console.ResetColor();

}

Console.WriteLine("Hello, World!");

var retryPolicy = Policy.Handle<Exception>()
    .WaitAndRetryAsync(
        retryCount: 1,
        sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
        onRetry: (exception, calculatedWaitDuration, context) =>
        {
            // Log or handle the retry attempt
            Console.WriteLine($"Retry attempt due to exception: {exception.Message}");
        }
    );

var circuitBreakerPolicy = Policy.Handle<Exception>()
    .CircuitBreakerAsync(
        exceptionsAllowedBeforeBreaking: 2,
        durationOfBreak: TimeSpan.FromSeconds(1),
        onBreak: (ex, breakDelay) =>
        {
            // Log or handle the circuit breaker opening
            Console.WriteLine("Circuit breaker opened. Requests are being blocked.");
        },
        onReset: () =>
        {
            // Log or handle the circuit breaker resetting
            Console.WriteLine("Circuit breaker reset. Requests are allowed again.");
        }
    );

var combinedPolicy = Policy.WrapAsync(circuitBreakerPolicy, retryPolicy);


await DoSomeWorkSeq(CircuitBreakerThreshold, CircuitBreakerDurationInMS, MaxConcurrentRequests, NumberOfRequests, new HttpClient());

Console.ReadLine();
Console.WriteLine("We are done!");


async Task DoSomeWork(int circuitBreakerThreshold, double circuitBreakerDurationMS, int maxConcurrentRequests, int numberOfRequests)
{
    var circuitBreakerPolicy = Policy.Handle<Exception>()
        .CircuitBreakerAsync(circuitBreakerThreshold, TimeSpan.FromMilliseconds(circuitBreakerDurationMS), OnCircuitBreakerOpened, OnCircuitBreakerReset, OnCircuitBreakerHalfOpen);

    var client = new HttpClient();
    var semaphore = new SemaphoreSlim(maxConcurrentRequests);

    var tasks = new List<Task>();
    var results = new List<(int Iteration, string ResponseContent)>(); // List to store the results

    for (int i = 0; i < numberOfRequests; i++)
    {
        int iteration = i; // Capture the value of i in a local variable
        int reqloopCount = loopCount++;
        string result = "init result";
        await semaphore.WaitAsync(); // Acquire a semaphore slot
        tasks.Add(Task.Run(async () =>
        {
            try
            {

                await circuitBreakerPolicy.ExecuteAsync(async () =>
                {
                    try
                    {
                        var request = new HttpRequestMessage(HttpMethod.Post, "https://asyncdemoweb.azurewebsites.net/api/remote/Results");
                        var content = new StringContent("{\r\n  \"loopCount\": " + reqloopCount + ",\r\n  \"maxTimeMS\": 100,\r\n  \"runTimeMS\": 0,\r\n  \"message\": \"string\",\r\n  \"resultValue\": \"string\"\r\n}", null, "application/json");
                        request.Content = content;
                        var response = await client.SendAsync(request);
                        response.EnsureSuccessStatusCode();
                        var responseContent = await response.Content.ReadAsStringAsync();

                        result = responseContent;
                    }
                    finally
                    {
                        semaphore.Release(); // Release the semaphore slot
                    }
                });
            }
            catch (BrokenCircuitException)
            {
                result = "Circuit breaker opened. Requests are being blocked.";
            }
            catch (Exception ex)
            {
                result = $"An error occurred: {ex.Message}";
            }
            results.Add((iteration, result));
        }));
    }

    // Wait for all tasks to complete
    await Task.WhenAll(tasks);

    // Print the results
    foreach (var result in results.OrderBy(o => o.Iteration))
    {
        Console.WriteLine($"Iteration: {result.Iteration}, Response: {result.ResponseContent}");
    }
}


async Task DoSomeWorkSeq(int circuitBreakerThreshold, double circuitBreakerTimeSpan, int maxConcurrentRequests, int numberOfRequests, HttpClient client)
{
    var circuitBreakerPolicy = Policy.Handle<Exception>()
        .CircuitBreakerAsync(circuitBreakerThreshold, TimeSpan.FromMicroseconds(circuitBreakerTimeSpan), OnCircuitBreakerOpened, OnCircuitBreakerReset, OnCircuitBreakerHalfOpen);
    var semaphore = new SemaphoreSlim(maxConcurrentRequests);

    var tasks = new List<Task>();
    var results = new List<(int Iteration, string ResponseContent)>(); // List to store the results


    for (int i = 0; i < numberOfRequests; i++)
    {
        try
        {

            await semaphore.WaitAsync(); // Acquire a semaphore slot
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    loopCount = RandomNumberGenerator.GetInt32(0, 15);

                    await circuitBreakerPolicy.ExecuteAsync(async () =>
                    {

                        await Task.Delay(TimeSpan.FromMilliseconds(RandomNumberGenerator.GetInt32(0, 100)));

                        try
                        {
                            var request = new HttpRequestMessage(HttpMethod.Post, "https://asyncdemoweb.azurewebsites.net/api/remote/Results");
                            var content = new StringContent("{\r\n  \"loopCount\": " + loopCount + ",\r\n  \"maxTimeMS\": 100,\r\n  \"runTimeMS\": 0,\r\n  \"message\": \"string\",\r\n  \"resultValue\": \"string\"\r\n}", null, "application/json");
                            request.Content = content;
                            var response = await client.SendAsync(request);
                            response.EnsureSuccessStatusCode();
                            Console.WriteLine(await response.Content.ReadAsStringAsync());
                            loopCount += 1;
                        }
                        finally
                        {
                            semaphore.Release(); // Release the semaphore slot
                        }
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.WriteLine($"\tLoop {i} LoopCount={loopCount}");
                        Console.ResetColor();

                    });
                }
                catch (BrokenCircuitException)
                {
                    Console.WriteLine("Circuit breaker opened. Requests are being blocked.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
            }));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
    // Wait for all tasks to complete
    await Task.WhenAll(tasks);

    Console.WriteLine($"DONE");

}
