using HttpClientUtility.Models;

namespace HttpClientUtility.SendService;

public interface IHttpClientSendService
{
    Task<HttpClientSendRequest<T>> HttpClientSendAsync<T>(HttpClientSendRequest<T> statusCall, CancellationToken ct);
}
