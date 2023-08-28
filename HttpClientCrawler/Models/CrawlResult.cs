using HtmlAgilityPack;
using HttpClientCrawler.Helpers;
using HttpClientDecorator.Models;

namespace HttpClientCrawler.Models;

public class CrawlResult : HttpClientSendRequest<string>
{

    public CrawlResult() : base()
    {
    }

    public CrawlResult(HttpClientSendRequest<string> statusCall) : base(statusCall)
    {
    }

    public List<string> CrawlLinks
    {
        get
        {
            ResponseLinks.Clear();
            if (ResponseHtmlDocument != null)
            {
                foreach (var link in ResponseHtmlDocument.DocumentNode
                    .Descendants("a")
                    .Select(a => SiteCrawlerHelpers.RemoveQueryAndOnPageLinks(a.GetAttributeValue("href", null), base.RequestPath))
                    .Where(link => !string.IsNullOrWhiteSpace(link))
                    )
                {
                    if (ResponseLinks.Contains(link))
                    {
                        continue;
                    }
                    if (SiteCrawlerHelpers.IsValidLink(link))
                    {
                        if (SiteCrawlerHelpers.IsSameDomain(link, base.RequestPath))
                        {
                            ResponseLinks.Add(link);
                        }
                    }
                }
            }
            return ResponseLinks;
        }
    }

    public HtmlDocument? ResponseHtmlDocument
    {
        get
        {
            if (string.IsNullOrWhiteSpace(ResponseResults))
            {
                return null;
            }
            try
            {
                HtmlDocument htmlDoc = new();
                htmlDoc.LoadHtml(ResponseResults);
                return htmlDoc;
            }
            catch
            {
                return null;
            }
        }
    }
    public List<string> ResponseLinks { get; } = new List<string>();

}
