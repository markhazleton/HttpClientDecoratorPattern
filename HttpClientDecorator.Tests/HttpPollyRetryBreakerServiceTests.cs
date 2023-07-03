using HttpClientDecorator.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace HttpClientDecorator.Tests;
[TestClass]
public class HttpPollyRetryBreakerServiceTests
{
    private Mock<ILogger<HttpPollyRetryBreakerService>> _loggerMock;
    private Mock<IHttpClientSendService> _serviceMock;

    [TestInitialize]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<HttpPollyRetryBreakerService>>();
        _serviceMock = new Mock<IHttpClientSendService>();
    }

    [TestMethod]
    public async Task HttpClientSendAsync_ShouldHandleExceptionAndLogErrors()

    {
        // Arrange
        var maxRetryAttempts = 3;
        var retryDelay = TimeSpan.FromSeconds(1);
        var circuitBreakerThreshold = 2;
        var circuitBreakerDuration = TimeSpan.FromMinutes(1);

        var options = new HttpPollyRetryBreakerOptions
        {
            MaxRetryAttempts = maxRetryAttempts,
            RetryDelay = retryDelay,
            CircuitBreakerThreshold = circuitBreakerThreshold,
            CircuitBreakerDuration = circuitBreakerDuration
        };

        var statusCall = new HttpClientSendResults<object>();
        var cancellationToken = CancellationToken.None;
        var exception = new Exception("Test exception");

        _serviceMock.SetupSequence(s => s.HttpClientSendAsync(It.IsAny<HttpClientSendResults<object>>(), cancellationToken))
            .ThrowsAsync(exception)
            .ReturnsAsync(statusCall);

        var service = new HttpPollyRetryBreakerService(
            _loggerMock.Object,
            _serviceMock.Object,
            options);

        // Act
        var result = await service.HttpClientSendAsync(statusCall, cancellationToken);

        // Assert
        //_serviceMock.Verify(s => s.HttpClientSendAsync(It.IsAny<HttpClientSendResults<object>>(), cancellationToken), Times.Exactly(2));
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

