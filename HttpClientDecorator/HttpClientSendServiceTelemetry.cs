using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace HttpClientDecorator;


/// <summary>
/// Class HttpClientSendServiceTelemetry adds telemetry to the IHttpClientRequestService implementation
/// </summary>
public class HttpClientSendServiceTelemetry : IHttpClientRequestService
{
    private readonly ILogger<HttpClientSendServiceTelemetry> _logger;
    private readonly IHttpClientRequestService _service;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpClientSendServiceTelemetry"/> class
    /// </summary>
    /// <param name="logger">ILogger instance</param>
    /// <param name="service">IHttpClientRequestService instance</param>
    public HttpClientSendServiceTelemetry(ILogger<HttpClientSendServiceTelemetry> logger, IHttpClientRequestService service)
    {
        _logger = logger;
        _service = service;
    }

    /// <summary>
    /// GetAsync performs a GET request and adds telemetry information to the response.
    /// </summary>
    /// <typeparam name="T">Result type of the GET request</typeparam>
    /// <param name="statusCall">HttpClientRequest instance</param>
    /// <returns>HttpClientRequest instance including telemetry information</returns>
    /// <param name="cts"></param>
    public async Task<HttpClientRequest<T>> HttpClientSendAsync<T>(HttpClientRequest<T> statusCall, CancellationToken ct)
    {
        Stopwatch sw = new();
        sw.Start();
        try
        {
            statusCall = await _service.HttpClientSendAsync(statusCall, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            statusCall.ErrorList.Add($"Telemetry:GetAsync:Exception:{ex.Message}");
            _logger.LogCritical("Telemetry:GetAsync:Exception", ex.Message);
        }
        sw.Stop();
        statusCall.ElapsedMilliseconds = sw.ElapsedMilliseconds;
        statusCall.CompletionDate = DateTime.Now;
        _logger.LogInformation("Telemetry:GetAsync:Attributes Updated");
        return statusCall;
    }
}
