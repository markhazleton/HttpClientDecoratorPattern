using HttpClientDecorator.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace HttpClientDecorator.Tests
{
    [TestClass]
    public class HttpGetCallServiceTelemetryTests
    {
        private Mock<IHttpClientSendService> _mockService;
        private Mock<ILogger<HttpGetCallServiceTelemetry>> _mockLogger;
        private HttpGetCallServiceTelemetry _telemetryService;

        [TestInitialize]
        public void TestSetup()
        {
            _mockService = new Mock<IHttpClientSendService>();
            _mockLogger = new Mock<ILogger<HttpGetCallServiceTelemetry>>();
            _telemetryService = new HttpGetCallServiceTelemetry(_mockLogger.Object, _mockService.Object);
        }

        [TestMethod]
        public async Task GetAsync_ReturnsExpectedResult()
        {
            // Arrange
            var expectedResponse = new HttpClientSendResults<string>
            {
                RequestPath = "https://example.com",
                ResponseResults = "OK",
                Retries = 0
            };
            _mockService.Setup(x => x.GetAsync(It.IsAny<HttpClientSendResults<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _telemetryService.GetAsync(expectedResponse, CancellationToken.None);

            // Assert
            Assert.AreEqual(expectedResponse.RequestPath, result.RequestPath);
            Assert.AreEqual(expectedResponse.ResponseResults, result.ResponseResults);
            Assert.AreEqual(expectedResponse.Retries, result.Retries);
            Assert.IsNotNull(result.CompletionDate);
            Assert.IsTrue(result.ElapsedMilliseconds >= 0);
            _mockLogger.Verify(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Never);
        }

        [TestMethod]
        public async Task GetAsync_LogsErrorAndReturnsOriginalResult_WhenExceptionIsThrown()
        {
            // Arrange
            var expectedResponse = new HttpClientSendResults<string>
            {
                RequestPath = "https://example.com",
                ResponseResults = "OK",
                Retries = 0
            };
            var expectedException = new Exception("Something went wrong");
            _mockService.Setup(x => x.GetAsync(It.IsAny<HttpClientSendResults<string>>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedException);

            // Act
            var result = await _telemetryService.GetAsync(expectedResponse, CancellationToken.None);

            // Assert
            Assert.AreEqual(expectedResponse.RequestPath, result.RequestPath);
            Assert.IsNull(result.ResponseResults);
            Assert.AreEqual(expectedResponse.Retries, result.Retries);
            Assert.IsNotNull(result.CompletionDate);
            Assert.IsTrue(result.ElapsedMilliseconds >= 0);
        }
    }
}
