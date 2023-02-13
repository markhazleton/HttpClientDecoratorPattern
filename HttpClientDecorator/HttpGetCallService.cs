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
    /// <param name="getCallResults">A container for the URL to make the GET request to, and the expected response data.</param>
    /// <returns>A container for the response data and any relevant error information.</returns>
    public async Task<HttpGetCallResults<T>> GetAsync<T>(HttpGetCallResults<T> getCallResults)
    {
        int retryCount = 0;
        int maxRetries = 3;

        if (getCallResults == null)
        {
            throw new ArgumentNullException(nameof(getCallResults), "The parameter 'getCallResults' cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(getCallResults.RequestPath))
        {
            throw new ArgumentException("The URL path specified in 'getCallResults' cannot be null or empty.", nameof(getCallResults));
        }

        string callResult = string.Empty;
        while (true)
        {
            try
            {
                getCallResults.Retries = retryCount;
                using var httpClient = _clientFactory.CreateClient();
                using var request = new HttpRequestMessage(HttpMethod.Get, getCallResults.RequestPath);
                request.Version = new Version(2, 0);
                request.Headers.ConnectionClose = false;
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
                using HttpResponseMessage response = await httpClient.SendAsync(request, cts.Token);
                response.EnsureSuccessStatusCode();
                callResult = await response.Content.ReadAsStringAsync();
                try
                {
                    getCallResults.ResponseResults = JsonSerializer.Deserialize<T>(callResult);
                    return getCallResults;
                }
                catch (Exception ex)
                {
                    _logger.LogCritical("HttpGetCallService:GetAsync:DeserializeException", ex.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical("HttpGetCallService:GetAsync:Exception", ex.Message);
                if (++retryCount >= maxRetries)
                {
                    throw;
                }
            }
        }
    }
}



