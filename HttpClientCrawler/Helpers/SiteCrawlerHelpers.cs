namespace HttpClientCrawler.Helpers;

public static class SiteCrawlerHelpers
{
    public static string GetDomainName(string url)
    {
        Uri uri = new(url);
        string host = uri.Host;
        if (host.StartsWith("www."))
        {
            host = host[4..];
        }
        return host;
    }
}
