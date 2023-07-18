using HttpClientDecorator.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace HttpClientDecorator.Tests;
[TestClass]
public class HttpPollyRetryBreakerServiceTests
{
    private Mock<ILogger<HttpClientSendServicePolly>> _loggerMock;
    private Mock<IHttpClientService> _serviceMock;

    [TestInitialize]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<HttpClientSendServicePolly>>();
        _serviceMock = new Mock<IHttpClientService>();
    }

    [TestMethod]
    public async Task HttpClientSendAsync_ShouldHandleExceptionAndLogErrors()

    {
        // Arrange
        var maxRetryAttempts = 3;
        var retryDelay = TimeSpan.FromSeconds(1);
        var circuitBreakerThreshold = 2;
        var circuitBreakerDuration = TimeSpan.FromMinutes(1);

        var options = new HttpClientSendPollyOptions
        {
            MaxRetryAttempts = maxRetryAttempts,
            RetryDelay = retryDelay,
            CircuitBreakerThreshold = circuitBreakerThreshold,
            CircuitBreakerDuration = circuitBreakerDuration
        };

        var statusCall = new HttpClientSendRequest<object>();
        var cancellationToken = CancellationToken.None;
        var exception = new Exception("Test exception");

        _serviceMock.SetupSequence(s => s.HttpClientSendAsync(It.IsAny<HttpClientSendRequest<object>>(), cancellationToken))
            .ThrowsAsync(exception)
            .ReturnsAsync(statusCall);

        var service = new HttpClientSendServicePolly(
            _loggerMock.Object,
            _serviceMock.Object,
            options);

        // Act
        var result = await service.HttpClientSendAsync(statusCall, cancellationToken);

        // Assert
        //_serviceMock.Verify(s => s.HttpClientSendAsync(It.IsAny<HttpClientSendRequest<object>>(), cancellationToken), Times.Exactly(2));
        //_loggerMock.Verify(
        //    l => l.Log(
        //        LogLevel.Information,
        //        It.IsAny<EventId>(),
        //        It.IsAny<It.IsAnyType>(),
        //        exception,
        //        (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
        //    ),
        //    Times.Never
        //);
        Assert.AreEqual(statusCall, result);
        CollectionAssert.Contains(result.ErrorList, $"Polly:GetAsync:Exception:Test exception");
    }
}

