﻿using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace HttpClientDecorator;

public class HttpClientSendPollyOptions
{
    public int MaxRetryAttempts { get; set; }
    public TimeSpan RetryDelay { get; set; }
    public int CircuitBreakerThreshold { get; set; }
    public TimeSpan CircuitBreakerDuration { get; set; }
}


public class HttpClientSendServicePolly : IHttpClientRequestService
{
    private readonly ILogger<HttpClientSendServicePolly> _logger;
    private readonly List<string> _errorList = new List<string>();
    private readonly IHttpClientRequestService _service;
    private readonly AsyncRetryPolicy _retryPolicy;
    private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;
    private readonly HttpClientSendPollyOptions _options;

    public HttpClientSendServicePolly(
        ILogger<HttpClientSendServicePolly> logger,
        IHttpClientRequestService service,
        HttpClientSendPollyOptions options)
    {
        _service = service;
        _logger = logger;
        _options = options;

        // Configure the retry policy
        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(options.MaxRetryAttempts, retryAttempt => options.RetryDelay,
                (exception, timespan, retryCount, context) =>
                {
                    // Optionally, you can log or handle the retry attempt here
                    _errorList.Add($"Polly.RetryPolicy Retries:{retryCount}, exception:{exception.Message}");
                });

        // Configure the circuit breaker policy
        _circuitBreakerPolicy = Policy
            .Handle<Exception>()
            .CircuitBreakerAsync(options.CircuitBreakerThreshold, options.CircuitBreakerDuration,
                (exception, duration) =>
                {
                    // Optionally, you can log or handle the circuit breaker state change here
                    _errorList.Add($"Polly.CircuitBreaker: duration{duration.TotalSeconds} exception:{exception.Message}");
                },
                () =>
                {
                    // Optionally, you can handle the circuit breaker being reset here
                    _errorList.Add($"Polly.CircuitBreaker: RESET");
                });
    }

    public async Task<HttpClientRequest<T>> HttpClientSendAsync<T>(HttpClientRequest<T> statusCall, CancellationToken ct)
    {
        // Wrap the GetAsync call with the circuit breaker policies
        try
        {
            statusCall = await _circuitBreakerPolicy.ExecuteAsync(() => _service.HttpClientSendAsync(statusCall, ct));
        }
        catch (Exception ex)
        {
            _logger.LogError($"Polly:CircuitBreaker:Exception:{ex.Message}");
            _errorList.Add($"Polly:GetAsync:Exception:{ex.Message}");
        }

        statusCall.ErrorList.AddRange(_errorList);

        _errorList.Clear();

        _logger.LogInformation("Polly:Call Completed");

        return statusCall;
    }
}