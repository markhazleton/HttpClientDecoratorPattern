using HttpClientCrawler.Crawler;

namespace HttpClientCrawler.Tests.Helpers;

[TestClass]
public class SiteCrawlerHelpersTests
{

    public const string RequestPath = "https://example.com";
    [TestMethod]
    public void GetDomainName_FullPath_ReturnsDomain()
    {
        string relativeLink = "/page";
        Uri combinedUri = new(new Uri(RequestPath), relativeLink);
        var result = SiteCrawlerHelpers.GetDomainName(combinedUri.ToString());
        Assert.AreEqual(result, "example.com");
    }

    [TestMethod]
    public void GetDomainName_WWWFullPath_ReturnsDomain()
    {
        string relativeLink = "/page";
        Uri combinedUri = new(new Uri("https://www.example.com"), relativeLink);
        var result = SiteCrawlerHelpers.GetDomainName(combinedUri.ToString());
        Assert.AreEqual(result, "example.com");
    }


    [TestMethod]
    public void IsSameDomain_RelativeLink_ReturnsTrue()
    {
        string relativeLink = "/page";
        bool result = SiteCrawlerHelpers.IsSameDomain(relativeLink, RequestPath);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void IsSameDomain_SameFullDomain_ReturnsTrue()
    {
        string url = "https://example.com/page";
        bool result = SiteCrawlerHelpers.IsSameDomain(url, RequestPath);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void IsSameDomain_DifferentFullDomain_ReturnsFalse()
    {
        string url = "https://otherdomain.com/page";
        bool result = SiteCrawlerHelpers.IsSameDomain(url, RequestPath);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsValidLink_ValidHtmlLink_ReturnsTrue()
    {
        string link = "/page.html";
        bool result = SiteCrawlerHelpers.IsValidLink(link);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void IsValidLink_InvalidImageLink_ReturnsFalse()
    {
        string link = "/image.png";
        bool result = SiteCrawlerHelpers.IsValidLink(link);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsValidLink_FullInvalidImageLink_ReturnsFalse()
    {
        string link = "https://www.frogsfolly.com/images/image.png";
        bool result = SiteCrawlerHelpers.IsValidLink(link);
        Assert.IsFalse(result);
    }
    [TestMethod]
    public void IsValidLink_FullInvalidCGILink_ReturnsFalse()
    {
        string link = "https://www.frogsfolly.com/cdn-cgi/login";
        bool result = SiteCrawlerHelpers.IsValidLink(link);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void RemoveQueryAndOnPageLinks_LinkWithQuery_RemovesQuery()
    {
        string link = "/page?param=value";
        string result = SiteCrawlerHelpers.RemoveQueryAndOnPageLinks(link, RequestPath);
        Uri combinedUri = new(new Uri(RequestPath), "/page");
        Assert.AreEqual(combinedUri.ToString(), result);
    }

    [TestMethod]
    public void RemoveQueryAndOnPageLinks_OnPageLink_RemovesQuery()
    {
        string link = "/page#top";
        string result = SiteCrawlerHelpers.RemoveQueryAndOnPageLinks(link, RequestPath);
        Uri combinedUri = new(new Uri(RequestPath), "/page");
        Assert.AreEqual(combinedUri.ToString(), result);
    }

}
