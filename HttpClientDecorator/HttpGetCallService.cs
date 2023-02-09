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
    public async Task<HttpGetCallResults> GetAsync<T>(HttpGetCallResults statusCall)
    {
        try
        {
            using var httpClient = _clientFactory.CreateClient();
            var response = await httpClient.GetAsync(statusCall.GetPath);
            response.EnsureSuccessStatusCode();
            var StatusResults = await response.Content.ReadAsStringAsync();
            try
            {
                statusCall.GetResults = JsonSerializer.Deserialize<T>(StatusResults);
            }
            catch (Exception ex)
            {
                _logger.LogCritical("HttpGetCallService:GetAsync:DeserializeException", ex.Message);
                statusCall.GetResults = JsonSerializer.Deserialize<dynamic>(StatusResults);
            }

        }
        catch (Exception ex)
        {
            _logger.LogCritical("HttpGetCallService:GetAsync:Exception", ex.Message);
        }
        return statusCall;
    }
}
