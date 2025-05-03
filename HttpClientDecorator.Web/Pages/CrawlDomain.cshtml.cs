using Microsoft.AspNetCore.Mvc;
using WebSpark.HttpClientUtility.Crawler;
using WebSpark.HttpClientUtility.RequestResult;

namespace HttpClientDecorator.Web.Pages;

public class CrawlDomainModel : PageModel
{
    [BindProperty]
    public bool IsCrawling { get; set; }
    [BindProperty]
    public string StartPath { get; set; } = string.Empty;
    [BindProperty]
    public int MaxPagesCrawled { get; set; } = 500; // Default maximum pages for crawling

    public ICollection<CrawlResult> CrawlResults = new List<CrawlResult>();
    public IHubContext<CrawlHub> hubContext { get; }
    private readonly SiteCrawler siteCrawler;

    public CrawlDomainModel(IHubContext<CrawlHub> hubContext, IHttpRequestResultService service, ILogger<SiteCrawler> logger)
    {
        this.hubContext = hubContext;
        siteCrawler = new SiteCrawler(hubContext, service, logger);
    }
    public async Task OnPostAsync(CancellationToken ct = default)
    {
        // Notify clients that crawling has started
        IsCrawling = true;
        await hubContext.Clients.All.SendAsync("UrlFound", $"CrawlAsync Is Started");
        // Start the crawling process
        try
        {
            CrawlerOptions crawlerOptions = new()
            {
                MaxPages = MaxPagesCrawled,
                MaxDepth = 3,
                RequestDelayMs = 10,
                SavePagesToDisk = false,
                OutputDirectory = null,
                UserAgent = "HttpClientCrawler/1.0",
                RespectRobotsTxt = false,
                ValidateHtml = false
            };
            var results = await siteCrawler.CrawlAsync(StartPath, crawlerOptions, ct: ct).ConfigureAwait(true);
            CrawlResults = results.CrawlResults;
        }
        finally
        {
            // Notify clients that crawling has finished
            IsCrawling = false;
            await hubContext.Clients.All.SendAsync("UrlFound", $"CrawlAsync Is Complete", cancellationToken: ct);

            // Pause for 3 secons to allow clients to see the final results
            await Task.Delay(3000, ct);
        }

    }
}
