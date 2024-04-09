using System;
using System.Net;

namespace HttpClientUtility;

/// <summary>
/// Represents the response content of an HTTP request.
/// </summary>
/// <typeparam name="T">The type of the content.</typeparam>
public class HttpResponseContent<T>
{
    /// <summary>
    /// Gets the status code of the HTTP response.
    /// </summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    /// Gets a value indicating whether the HTTP response is successful.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets the error message of the HTTP response, if any.
    /// </summary>
    public string? ErrorMessage { get; }

    /// <summary>
    /// Gets the content of the HTTP response.
    /// </summary>
    public T? Content { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpResponseContent{T}"/> class.
    /// </summary>
    /// <param name="content">The content of the HTTP response.</param>
    /// <param name="errorMessage">The error message of the HTTP response.</param>
    /// <param name="statusCode">The status code of the HTTP response.</param>
    /// <param name="isSuccess">A value indicating whether the HTTP response is successful.</param>
    private HttpResponseContent(T? content, string? errorMessage, HttpStatusCode statusCode, bool isSuccess)
    {
        StatusCode = statusCode;
        IsSuccess = isSuccess;
        if(isSuccess)
            Content = content;

        ErrorMessage = !isSuccess ? errorMessage : null;
    }

    /// <summary>
    /// Creates a success instance of the <see cref="HttpResponseContent{T}"/> class.
    /// </summary>
    /// <param name="content">The content of the HTTP response.</param>
    /// <param name="statusCode">The status code of the HTTP response.</param>
    /// <returns>A success instance of the <see cref="HttpResponseContent{T}"/> class.</returns>
    public static HttpResponseContent<T> Success(T content, HttpStatusCode statusCode) =>
        new(content, null, statusCode, true);

    /// <summary>
    /// Creates a failure instance of the <see cref="HttpResponseContent{T}"/> class.
    /// </summary>
    /// <param name="errorMessage">The error message of the HTTP response.</param>
    /// <param name="statusCode">The status code of the HTTP response.</param>
    /// <returns>A failure instance of the <see cref="HttpResponseContent{T}"/> class.</returns>
    public static HttpResponseContent<T> Failure(string errorMessage, HttpStatusCode statusCode) =>
        new(default, errorMessage, statusCode, false);
}
