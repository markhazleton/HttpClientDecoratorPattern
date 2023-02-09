namespace HttpClientDecorator.Interfaces;

public interface IHttpGetCallService
{
    Task<HttpGetCallResults> GetAsync<T>(HttpGetCallResults statusCall);
}
