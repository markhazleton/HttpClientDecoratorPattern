using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace HttpClientDecorator;

public class HttpPollyRetryBreakerService : IHttpClientSendService
{
    private readonly ILogger<HttpPollyRetryBreakerService> _logger;
    private readonly ThreadLocal<List<string>> _errorList = new(() => new List<string>());
    private readonly IHttpClientSendService _service;
    private readonly AsyncRetryPolicy _retryPolicy;
    private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;

    public HttpPollyRetryBreakerService(ILogger<HttpPollyRetryBreakerService> logger, IHttpClientSendService service)
    {
        _service = service;
        _logger = logger;

        // Configure the retry policy
        _retryPolicy = Policy
             .Handle<Exception>()
             .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(1),
                 (exception, timespan, retryCount, context) =>
                 {
                     // Optionally, you can log or handle the retry attempt here
                     _errorList.Value.Add($"Polly.RetryPolicy Retries:{retryCount}");
                 });

        // Configure the circuit breaker policy
        _circuitBreakerPolicy = Policy
            .Handle<Exception>()
            .CircuitBreakerAsync(3, TimeSpan.FromSeconds(1),
                (exception, duration) =>
                {
                    // Optionally, you can log or handle the circuit breaker state change here
                    _errorList.Value.Add($"Polly.CircuitBreaker: duration{duration.TotalSeconds} exception:{exception.Message}");
                },
                () =>
                {
                    // Optionally, you can handle the circuit breaker being reset here
                    _errorList.Value.Add($"Polly.CircuitBreaker: RESET");
                });
    }

    public async Task<HttpClientSendResults<T>> GetAsync<T>(HttpClientSendResults<T> statusCall, CancellationToken ct)
    {
        // Wrap the GetAsync call with the retry and circuit breaker policies
        HttpClientSendResults<T> response = new();
        try
        {
            response = await Policy.WrapAsync(_retryPolicy, _circuitBreakerPolicy)
                            .ExecuteAsync(() => _service.GetAsync(statusCall, ct));
        }
        catch (Exception ex)
        {
            _errorList.Value.Add($"Polly:GetAsync:Exception:{ex.Message}");
        }
        response.ErrorList.AddRange(_errorList.Value);
        _errorList.Value.Clear();
        return response;
    }




}
