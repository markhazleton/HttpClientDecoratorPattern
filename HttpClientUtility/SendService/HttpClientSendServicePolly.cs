﻿using HttpClientUtility.Models;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace HttpClientUtility.SendService;


/// <summary>
/// Service for sending HTTP requests with Polly policies for retry and circuit breaker.
/// </summary>
public class HttpClientSendServicePolly : IHttpClientSendService
{
    private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;
    private readonly List<string> _errorList = [];
    private readonly ILogger<HttpClientSendServicePolly> _logger;
    private readonly HttpClientSendPollyOptions _options;
    private readonly AsyncRetryPolicy _retryPolicy;
    private readonly IHttpClientSendService _service;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpClientSendServicePolly"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="service">The HTTP client send service instance.</param>
    /// <param name="options">The Polly options for retry and circuit breaker policies.</param>
    /// <exception cref="ArgumentNullException">Thrown when any of the parameters are null.</exception>
    public HttpClientSendServicePolly(
        ILogger<HttpClientSendServicePolly>? logger,
        IHttpClientSendService? service,
        HttpClientSendPollyOptions? options)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? throw new ArgumentNullException(nameof(options));

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

    /// <summary>
    /// Sends an HTTP request asynchronously with Polly policies for retry and circuit breaker.
    /// </summary>
    /// <typeparam name="T">The type of the response result.</typeparam>
    /// <param name="statusCall">The HTTP client send request.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The HTTP client send request with the response result.</returns>
    public async Task<HttpClientSendRequest<T>> HttpClientSendAsync<T>(HttpClientSendRequest<T> statusCall, CancellationToken ct)
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

        return statusCall;
    }
}
