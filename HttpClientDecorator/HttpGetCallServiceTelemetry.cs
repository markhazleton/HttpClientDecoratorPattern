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
    public async Task<HttpClientSendResults<T>> GetAsync<T>(HttpClientSendResults<T> statusCall, CancellationToken ct)
    {
        Stopwatch sw = new();
        sw.Start();
        var response = new HttpClientSendResults<T>(statusCall);
        try
        {
            response = await _service.GetAsync(statusCall, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            response.ErrorList.Add($"Telemetry:GetAsync:Exception:{ex.Message}");
            _logger.LogCritical("Telemetry:GetAsync:Exception", ex.Message);
        }
        sw.Stop();
        response.ElapsedMilliseconds = sw.ElapsedMilliseconds;
        response.CompletionDate = DateTime.Now;
        return response;
    }
}
