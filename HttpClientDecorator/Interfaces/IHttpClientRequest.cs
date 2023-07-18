namespace HttpClientDecorator.Interfaces;

public interface IHttpClientRequest<T>
{
    DateTime? CompletionDate { get; set; }
    long ElapsedMilliseconds { get; set; }
    List<string> ErrorList { get; set; }
    int Id { get; set; }
    int Iteration { get; set; }
    string RequestPath { get; set; }
    T? ResponseResults { get; set; }
    int Retries { get; set; }
}
