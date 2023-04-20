using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.Text.Json;

namespace HttpClientDecorator.Tests
{
    [TestClass]
    public class HttpGetCallServiceTests
    {
        private HttpGetCallService _httpGetCallService;
        private Mock<ILogger<HttpGetCallService>> _loggerMock;
        private Mock<IHttpClientFactory> _httpClientFactoryMock;
        private Mock<HttpClient> _httpClientMock;

        [TestInitialize]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<HttpGetCallService>>();
            _httpClientMock = new Mock<HttpClient>();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(_httpClientMock.Object);
            _httpGetCallService = new HttpGetCallService(_loggerMock.Object, _httpClientFactoryMock.Object);
        }

        [TestMethod]
        public async Task GetAsync_NullGetCallResults_ThrowsArgumentNullException()
        {
            // Arrange
            HttpGetCallResults<object> getCallResults = null;

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => _httpGetCallService.GetAsync(getCallResults, CancellationToken.None));
        }

        [TestMethod]
        public async Task GetAsync_EmptyRequestPath_ThrowsArgumentException()
        {
            // Arrange
            var getCallResults = new HttpGetCallResults<object>
            {
                RequestPath = string.Empty
            };

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _httpGetCallService.GetAsync(getCallResults, CancellationToken.None));
        }

        [TestMethod]
        public async Task GetAsync_Success_ReturnsHttpGetCallResultsWithResponseData()
        {
            // Arrange
            var getCallResults = new HttpGetCallResults<string>
            {
                RequestPath = "http://example.com",
                ResponseResults = "success"
            };
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(getCallResults.ResponseResults))
            };
            _httpClientMock.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(httpResponseMessage);

            // Act
            var result = await _httpGetCallService.GetAsync(getCallResults, CancellationToken.None);

            // Assert
            Assert.AreEqual(getCallResults.ResponseResults, result.ResponseResults);
            Assert.IsNull(result.ErrorMessage);
            Assert.AreEqual(0, result.Retries);
        }

        [TestMethod]
        public async Task GetAsync_DeserializeException_LogsCriticalErrorAndReturnsHttpGetCallResultsWithError()
        {
            // Arrange
            var getCallResults = new HttpGetCallResults<string>
            {
                RequestPath = "http://example.com",
                ResponseResults = "invalid-json"
            };
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(getCallResults.ResponseResults)
            };
            _httpClientMock.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(httpResponseMessage);

            // Act
            var result = await _httpGetCallService.GetAsync(getCallResults, CancellationToken.None);

            // Assert
            Assert.AreEqual(result.ResponseResults, "invalid-json");
            Assert.IsNotNull(result.ErrorMessage);
            Assert.IsTrue(result.ErrorMessage.StartsWith("HttpGetCallService:GetAsync:DeserializeException"));
        }
    }
}