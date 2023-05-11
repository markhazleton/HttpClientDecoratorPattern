namespace HttpClientDecorator.Interfaces;

public interface IHttpGetCallResults<T>
{
    DateTime? CompletionDate { get; set; }
    long ElapsedMilliseconds { get; set; }
    string? ErrorMessage { get; set; }
    int Id { get; set; }
    int Iteration { get; set; }
    string RequestPath { get; set; }
    T? ResponseResults { get; set; }
    int Retries { get; set; }
}
