namespace HttpClientCrawler.Crawler;

public interface ISiteCrawler
{
    Task<ICollection<CrawlResult>> Crawl(int maxCrawlDepth, string StartPath, CancellationToken ct = default);
}
