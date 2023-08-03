using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace HttpClientDecorator;
public class HttpClientSendService : IHttpClientService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HttpClientSendService> _logger;

    public HttpClientSendService(ILogger<HttpClientSendService> logger, HttpClient httpClient)
    {
        _httpClient = httpClient;
        _logger = logger;
    }
    /// <summary>
    /// Makes a request to the specified URL and returns the response.
    /// </summary>
    /// <typeparam name="T">The type of the expected response data.</typeparam>
    /// <param name="httpSendResults">A container for the URL to make the GET request to, and the expected response data.</param>
    /// <returns>A container for the response data and any relevant error information.</returns>
    /// <param name="ct"></param>
    public async Task<HttpClientSendRequest<T>> HttpClientSendAsync<T>(HttpClientSendRequest<T> httpSendResults, CancellationToken ct)
    {
        if (httpSendResults == null)
        {
            throw new ArgumentNullException(nameof(httpSendResults), "The parameter 'httpSendResults' cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(httpSendResults.RequestPath))
        {
            throw new ArgumentException("The URL path specified in 'httpSendResults' cannot be null or empty.", nameof(httpSendResults));
        }

        using var request = new HttpRequestMessage(httpSendResults.RequestMethod, httpSendResults.RequestPath);
        request.Version = new Version(2, 0);
        request.Headers.ConnectionClose = false;
        if (httpSendResults.RequestBody != null)
        {
            request.Content = httpSendResults.RequestBody;
        }
        using HttpResponseMessage response = await _httpClient.SendAsync(request, ct).ConfigureAwait(false);
        httpSendResults.StatusCode = response.StatusCode;

        response.EnsureSuccessStatusCode();
        string callResult = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

        if (typeof(T) == typeof(string))
        {
            httpSendResults.ResponseResults = (T)(object)callResult;
        }
        else
        {
            try
            {
                httpSendResults.ResponseResults = JsonSerializer.Deserialize<T>(callResult);
            }
            catch (Exception ex)
            {
                httpSendResults.ErrorList.Add($"HttpClientSendRequest:GetAsync:DeserializeException:{ex.Message}");
                _logger.LogCritical("HttpClientSendRequest:GetAsync:DeserializeException {ex.Message}", ex.Message);
            }
        }
        return httpSendResults;
    }
}



