# HttpClient Decorator Pattern for .NET

[![Build and deploy ASP.Net Core app to Azure Web App - HttpClientDecoratorPattern](https://github.com/markhazleton/HttpClientDecoratorPattern/actions/workflows/main_httpclientdecoratorpattern.yml/badge.svg)](https://github.com/markhazleton/HttpClientDecoratorPattern/actions/workflows/main_httpclientdecoratorpattern.yml)
[![NuGet](https://img.shields.io/nuget/v/WebSpark.HttpClientUtility.svg)](https://www.nuget.org/packages/WebSpark.HttpClientUtility/)

## 🚀 Enhancing HttpClient with Telemetry, Caching, and Circuit Breaking

This repository serves as a demonstration site for the public NuGet package [WebSpark.HttpClientUtility](https://www.nuget.org/packages/WebSpark.HttpClientUtility/), showcasing how to implement the Decorator Design Pattern in C# to enhance HttpClient functionality with telemetry, caching, and circuit breaking capabilities. It provides a practical, real-world example for .NET developers working with HTTP services.

## 📋 Overview

The **Decorator Pattern** is a structural design pattern that allows behavior to be added to individual objects without affecting the behavior of other objects from the same class. This repository demonstrates how this pattern is implemented in the WebSpark.HttpClientUtility package and can be applied to the HttpClient class to add cross-cutting concerns like:

- **Telemetry**: Track request/response times and sizes
- **Caching**: Reduce redundant API calls
- **Circuit Breaking**: Prevent cascading failures during API outages
- **Polly Integration**: Implement resilience policies

By following this pattern, the original HttpClient class remains untouched while additional functionality is layered through decorator classes, making the codebase more maintainable and adhering to the Open/Closed principle.

## 🌟 Features

1. **Telemetry-Enhanced HttpClient**: Track performance metrics for all HTTP requests
   - Response time tracking
   - Request/response size monitoring
   - Detailed logging with correlation IDs

2. **Advanced API Interactions**:
   - **Joke API**: Demonstrates simple API consumption with telemetry
     ![Joke Page](https://raw.githubusercontent.com/markhazleton/HttpClientDecoratorPattern/main/Images/JokeRazorPageResults.JPG)

   - **Concurrent API Calls**: Shows how to manage multiple parallel requests with SemaphoreSlim
     ![Many Calls](https://raw.githubusercontent.com/markhazleton/HttpClientDecoratorPattern/main/Images/ListPageResults.JPG)

   - **Circuit Breaking**: Protects your application from API failures

3. **Additional Patterns**:
   - Implementation of SemaphoreSlim for controlled concurrent requests
   - Memory caching for API responses
   - SignalR integration for real-time updates

## 💻 Technologies Used

- ASP.NET Core (.NET 9.0)
- C# 12
- Razor Pages
- SignalR
- Polly for resilience policies
- [WebSpark.HttpClientUtility](https://www.nuget.org/packages/WebSpark.HttpClientUtility/) package providing the core implementation

## 📦 WebSpark.HttpClientUtility NuGet Package

This demonstration site uses the WebSpark.HttpClientUtility NuGet package which provides:

- `HttpRequestResult<T>` - Core class for handling HTTP requests with built-in telemetry
- `IHttpRequestResultService` - Interface for the HTTP request service
- `HttpRequestResultService` - Base implementation of the service
- Decorator implementations:
  - `HttpRequestResultServiceTelemetry` - Adds telemetry capabilities
  - `HttpRequestResultServiceCache` - Adds caching capabilities
  - `HttpRequestResultServicePolly` - Adds resilience policies with Polly

To use the package in your own projects:

```
dotnet add package WebSpark.HttpClientUtility
```

## 🧩 Architecture

The implementation leverages the WebSpark.HttpClientUtility package and follows a clean decorator pattern architecture with the following components:

### Core Interface from WebSpark.HttpClientUtility

```csharp
public interface IHttpRequestResultService
{
    Task<HttpRequestResult<T>> HttpSendRequestResultAsync<T>(HttpRequestResult<T> requestResult, CancellationToken ct = default);
}
```

### Base Implementation from WebSpark.HttpClientUtility

```csharp
public class HttpRequestResultService : IHttpRequestResultService
{
    private readonly HttpClient _httpClient;
    
    public HttpRequestResultService(ILogger<HttpRequestResultService> logger, 
                                  IConfiguration configuration, 
                                  HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<HttpRequestResult<T>> HttpSendRequestResultAsync<T>(HttpRequestResult<T> requestResult, 
                                                                       CancellationToken ct = default)
    {
       // Base implementation of HTTP request handling
    }
}
```

### Decorator Implementations from WebSpark.HttpClientUtility

```csharp
// Telemetry Decorator
public class HttpRequestResultServiceTelemetry : IHttpRequestResultService
{
    private readonly IHttpRequestResultService _decoratedService;
    
    public HttpRequestResultServiceTelemetry(ILogger<HttpRequestResultServiceTelemetry> logger,
                                         IHttpRequestResultService decoratedService)
    {
        _decoratedService = decoratedService;
    }
    
    public async Task<HttpRequestResult<T>> HttpSendRequestResultAsync<T>(HttpRequestResult<T> requestResult, 
                                                                       CancellationToken ct = default)
    {
        // Add telemetry before and after the decorated service call
    }
}

// Caching Decorator
public class HttpRequestResultServiceCache : IHttpRequestResultService
{
    private readonly IHttpRequestResultService _decoratedService;
    private readonly IMemoryCache _cache;
    
    // Implementation with caching capability
}

// Polly Resilience Decorator
public class HttpRequestResultServicePolly : IHttpRequestResultService
{
    private readonly IHttpRequestResultService _decoratedService;
    
    // Implementation with Polly retry and circuit breaker policies
}
```

### Service Registration Using WebSpark.HttpClientUtility Classes

```csharp
// Registering the layered decorators in Program.cs
builder.Services.AddSingleton(serviceProvider =>
{
    // Base service from WebSpark.HttpClientUtility
    IHttpRequestResultService baseService = new HttpRequestResultService(
        /* dependencies */);

    // Add Polly resilience policies from WebSpark.HttpClientUtility
    IHttpRequestResultService pollyService = new HttpRequestResultServicePolly(
        /* dependencies */,
        baseService);

    // Add telemetry from WebSpark.HttpClientUtility
    IHttpRequestResultService telemetryService = new HttpRequestResultServiceTelemetry(
        /* dependencies */,
        pollyService);

    // Add caching from WebSpark.HttpClientUtility
    IHttpRequestResultService cacheService = new HttpRequestResultServiceCache(
        telemetryService,
        /* dependencies */);

    return cacheService;
});
```

## 🚦 Getting Started

1. **Prerequisites**:
   - Visual Studio 2022 or newer
   - .NET 9.0 SDK or newer

2. **Installation**:

   ```
   git clone https://github.com/markhazleton/HttpClientDecoratorPattern.git
   cd HttpClientDecoratorPattern
   ```

3. **Run the application**:
   - Open the solution in Visual Studio
   - Build and run the HttpClientDecorator.Web project
   - Navigate through the different examples

## 🔌 API Integration Examples Using WebSpark.HttpClientUtility

This project demonstrates integration with several APIs using the WebSpark.HttpClientUtility package:

- **JokeAPI**: Public API for retrieving jokes (<https://jokeapi.dev/>)
- **Art Institute API**: Example of consuming art data
- **NASA Picture API**: Example of image API integration

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

Please review the [contributing guidelines](https://github.com/markhazleton/HttpClientDecoratorPattern/blob/main/CONTRIBUTING.md) before making pull requests.

## 📚 Learn More

- [Read the Blog Post](https://markhazleton.com/decorator-pattern-http-client.html)
- [WebSpark.HttpClientUtility on NuGet](https://www.nuget.org/packages/WebSpark.HttpClientUtility/)
- [Live Demo](https://httpclientdecoratorpattern.azurewebsites.net/)

## 📜 License

MIT © [Mark Hazleton](https://markhazleton.com)
