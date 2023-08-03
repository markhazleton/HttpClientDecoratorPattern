namespace HttpClientCrawler.Helpers
{
    public interface ISiteCrawler
    {
        Task Crawl(int maxCrawlDepth, CancellationToken ct = default);
        Task ExportToCSV(string filePath);
    }
}
