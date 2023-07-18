
namespace HttpClientDecorator.Interfaces;

public interface IHttpClientRequestService
{
    Task<HttpClientRequest<T>> HttpClientSendAsync<T>(HttpClientRequest<T> statusCall, CancellationToken ct);
}
