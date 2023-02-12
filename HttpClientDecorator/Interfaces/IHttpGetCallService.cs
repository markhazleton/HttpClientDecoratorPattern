
namespace HttpClientDecorator.Interfaces;

public interface IHttpGetCallService
{
    Task<HttpGetCallResults<T>> GetAsync<T>(HttpGetCallResults<T> statusCall);
}
