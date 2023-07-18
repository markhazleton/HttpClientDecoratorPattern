using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.Text.Json;

namespace HttpClientDecorator.Tests
{
    [TestClass]
    public class HttpGetCallServiceTests
    {
        private HttpClientSendService _httpGetCallService;
        private Mock<ILogger<HttpClientSendService>> _loggerMock;
        private Mock<IHttpClientFactory> _httpClientFactoryMock;
        private Mock<HttpClient> _httpClientMock;

        [TestInitialize]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<HttpClientSendService>>();
            _httpClientMock = new Mock<HttpClient>();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(_httpClientMock.Object);
            _httpGetCallService = new HttpClientSendService(_loggerMock.Object, _httpClientFactoryMock.Object);
        }

        [TestMethod]
        public async Task GetAsync_NullGetCallResults_ThrowsArgumentNullException()
        {
            // Arrange
            HttpClientSendRequest<object> getCallResults = null;

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => _httpGetCallService.HttpClientSendAsync(getCallResults, CancellationToken.None));
        }

        [TestMethod]
        public async Task GetAsync_EmptyRequestPath_ThrowsArgumentException()
        {
            // Arrange
            var getCallResults = new HttpClientSendRequest<object>
            {
                RequestPath = string.Empty
            };

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _httpGetCallService.HttpClientSendAsync(getCallResults, CancellationToken.None));
        }

        [TestMethod]
        public async Task GetAsync_Success_ReturnsHttpGetCallResultsWithResponseData()
        {
            // Arrange
            var getCallResults = new HttpClientSendRequest<string>
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
            var result = await _httpGetCallService.HttpClientSendAsync(getCallResults, CancellationToken.None);

            // Assert
            Assert.AreEqual(getCallResults.ResponseResults, result.ResponseResults);
            Assert.AreEqual(result.ErrorList.Count, 0);
            Assert.AreEqual(0, result.Retries);
        }

        [TestMethod]
        public async Task GetAsync_DeserializeException_LogsCriticalErrorAndReturnsHttpGetCallResultsWithError()
        {
            // Arrange
            var getCallResults = new HttpClientSendRequest<string>
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
            var result = await _httpGetCallService.HttpClientSendAsync(getCallResults, CancellationToken.None);

            // Assert
            Assert.AreEqual(result.ResponseResults, "invalid-json");
            Assert.AreEqual(result.ErrorList.Count, 1);
            Assert.IsTrue(result.ErrorList.FirstOrDefault().StartsWith("HttpClientSendRequest:GetAsync:DeserializeException"));
        }
    }
}