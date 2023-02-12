using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace HttpClientDecorator;

public class HttpGetCallService : IHttpGetCallService
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly ILogger<HttpGetCallService> _logger;

    public HttpGetCallService(ILogger<HttpGetCallService> logger, IHttpClientFactory httpClientFactory)
    {
        _clientFactory = httpClientFactory;
        _logger = logger;
    }
    /// <summary>
    /// Makes a GET request to the specified URL and returns the response.
    /// </summary>
    /// <typeparam name="T">The type of the expected response data.</typeparam>
    /// <param name="statusCall">A container for the URL to make the GET request to, and the expected response data.</param>
    /// <returns>A container for the response data and any relevant error information.</returns>
    public async Task<HttpGetCallResults<T>> GetAsync<T>(HttpGetCallResults<T> statusCall)
    {
        if (statusCall == null)
        {
            throw new ArgumentNullException(nameof(statusCall), "The parameter 'statusCall' cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(statusCall.RequestPath))
        {
            throw new ArgumentException("The URL path specified in 'statusCall' cannot be null or empty.", nameof(statusCall));
        }

        try
        {
            using var httpClient = _clientFactory.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Get, statusCall.RequestPath);
            using var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var statusResults = await response.Content.ReadAsStringAsync();
            try
            {
                statusCall.ResponseResults = JsonSerializer.Deserialize<T>(statusResults);
            }
            catch (Exception ex)
            {
                _logger.LogCritical("HttpGetCallService:GetAsync:DeserializeException", ex.Message);
            }

        }
        catch (Exception ex)
        {
            _logger.LogCritical("HttpGetCallService:GetAsync:Exception", ex.Message);
        }

        return statusCall;
    }

}
