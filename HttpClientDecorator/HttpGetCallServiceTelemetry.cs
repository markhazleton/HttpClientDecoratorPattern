using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace HttpClientDecorator;


/// <summary>
/// Class HttpGetCallServiceTelemetry adds telemetry to the IHttpClientSendService implementation
/// </summary>
public class HttpGetCallServiceTelemetry : IHttpClientSendService
{
    private readonly ILogger<HttpGetCallServiceTelemetry> _logger;
    private readonly IHttpClientSendService _service;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpGetCallServiceTelemetry"/> class
    /// </summary>
    /// <param name="logger">ILogger instance</param>
    /// <param name="service">IHttpClientSendService instance</param>
    public HttpGetCallServiceTelemetry(ILogger<HttpGetCallServiceTelemetry> logger, IHttpClientSendService service)
    {
        _logger = logger;
        _service = service;
    }

    /// <summary>
    /// GetAsync performs a GET request and adds telemetry information to the response.
    /// </summary>
    /// <typeparam name="T">Result type of the GET request</typeparam>
    /// <param name="statusCall">HttpClientSendResults instance</param>
    /// <returns>HttpClientSendResults instance including telemetry information</returns>
    /// <param name="cts"></param>
    public async Task<HttpClientSendResults<T>> HttpClientSendAsync<T>(HttpClientSendResults<T> statusCall, CancellationToken ct)
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
        return statusCall;
    }
}
