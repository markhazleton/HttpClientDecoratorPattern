
namespace HttpClientDecorator.Interfaces;

public interface IHttpClientSendService
{
    Task<HttpClientSendResults<T>> HttpClientSendAsync<T>(HttpClientSendResults<T> statusCall, CancellationToken ct);
}
