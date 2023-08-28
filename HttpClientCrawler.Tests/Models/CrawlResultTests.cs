using HtmlAgilityPack;
using HttpClientCrawler.Models;

namespace HttpClientCrawler.Tests.Models;

[TestClass]
public class CrawlResultTests
{
    public const string RequestPath = "https://example.com";

    [TestMethod]
    public void CrawlLinks_ValidHtmlDocument_ReturnsListOfLinks()
    {
        var crawlResult = new CrawlResult
        {
            RequestPath = RequestPath,
            ResponseResults = "<html><a href=\"/page\">Link</a><a href=\"/page\">page</a></html>"
        };

        List<string> links = crawlResult.CrawlLinks;
        Uri combinedUri = new(new Uri(RequestPath), "/page");
        Assert.AreEqual(1, links.Count);
        Assert.AreEqual(combinedUri.ToString(), links[0]);
    }

    [TestMethod]
    public void ResponseHtmlDocument_ValidHtml_ReturnsHtmlDocument()
    {
        var crawlResult = new CrawlResult
        {
            ResponseResults = "<html><body><p>Hello</p></body></html>"
        };
        HtmlDocument? htmlDocument = crawlResult.ResponseHtmlDocument;
        Assert.IsNotNull(htmlDocument);
        Assert.AreEqual("Hello", htmlDocument.DocumentNode.SelectSingleNode("//p").InnerText);
    }

    [TestMethod]
    public void ResponseHtmlDocument_BadHtml_ReturnsNull()
    {
        var crawlResult = new CrawlResult
        {
            ResponseResults = "~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"
        };
        HtmlDocument? htmlDocument = crawlResult.ResponseHtmlDocument;
        Assert.IsNotNull(htmlDocument);
    }

    [TestMethod]
    public void ResponseHtmlDocument_NullHtml_ReturnsNull()
    {
        var crawlResult = new CrawlResult
        {
            ResponseResults = null
        };
        HtmlDocument? htmlDocument = crawlResult.ResponseHtmlDocument;
        Assert.IsNull(htmlDocument);
    }


}
