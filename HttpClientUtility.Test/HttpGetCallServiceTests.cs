using HttpClientUtility.Models;
using HttpClientUtility.SendService;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;

namespace HttpClientDecorator.Tests;





[TestClass]
public class HttpGetCallServiceTests
{
    private Mock<ILogger<HttpClientSendService>>? _loggerMock;
    private Mock<HttpClient>? _httpClientMock;
    private HttpClientSendService? _httpClientSendService;

    [TestInitialize]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<HttpClientSendService>>();
        _httpClientMock = new Mock<HttpClient>();
        _httpClientSendService = new HttpClientSendService(_loggerMock.Object, _httpClientMock.Object);
    }
    [TestMethod]
    public void CreateHttpRequest_Should_Create_HttpRequest()
    {
        // Arrange

        var requestBody = new StringContent("Hello, World!");

        var httpSendResults = new HttpClientSendRequest<object>()
        {
            RequestBody = requestBody,
            RequestMethod = HttpMethod.Post,
            RequestPath = "https://localhost/api/SampleData/EchoAsync"
        };
        var httpClientSendService = new HttpClientSendService(Mock.Of<ILogger<HttpClientSendService>>(), new HttpClient());

        // Act
        var httpRequest = httpClientSendService.CreateHttpRequest(httpSendResults);

        // Assert
        Assert.IsNotNull(httpRequest);
        Assert.AreEqual(httpSendResults.RequestPath, httpRequest.RequestUri.AbsoluteUri);
        Assert.AreEqual(httpSendResults.RequestMethod, httpRequest.Method);
        Assert.IsFalse(httpRequest.Headers.ConnectionClose.GetValueOrDefault(true));
    }
    [TestMethod]
    public async Task HttpClientSendAsync_SuccessfulRequest_ReturnsResponse()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"key\":\"value\"}")
        };
        _httpClientMock.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        var httpSendResults = new HttpClientSendRequest<string>
        {
            RequestPath = "https://example.com/api/resource",
            RequestMethod = HttpMethod.Get
        };

        // Act
        var result = await _httpClientSendService.HttpClientSendAsync(httpSendResults, CancellationToken.None);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        Assert.AreEqual("{\"key\":\"value\"}", result.ResponseResults);
    }


}
