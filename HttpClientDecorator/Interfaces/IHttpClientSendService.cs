
namespace HttpClientDecorator.Interfaces;

public interface IHttpClientSendService
{
    Task<HttpClientSendResults<T>> GetAsync<T>(HttpClientSendResults<T> statusCall, CancellationToken ct);
}
