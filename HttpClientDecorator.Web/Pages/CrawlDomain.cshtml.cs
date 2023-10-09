using HttpClientCrawler.Crawler;
using Microsoft.AspNetCore.Mvc;

namespace HttpClientDecorator.Web.Pages;
public class CrawlDomainModel : PageModel
{
    [BindProperty]
    public bool IsCrawling { get; set; }
    [BindProperty]
    public string StartPath { get; set; }
    [BindProperty]
    public int MaxPagesCrawled { get; set; } = 500; // Default maximum pages for crawling

    public ICollection<CrawlResult> CrawlResults;
    public IHubContext<CrawlHub> hubContext { get; }
    private readonly SiteCrawler siteCrawler;

    public CrawlDomainModel(IHubContext<CrawlHub> hubContext, IHttpClientService service, ILogger<SiteCrawler> logger)
    {
        this.hubContext = hubContext;
        siteCrawler = new SiteCrawler(hubContext, service, logger);
    }
    public async Task OnPostAsync()
    {
        // Notify clients that crawling has started
        IsCrawling = true;
        await hubContext.Clients.All.SendAsync("UrlFound", $"CrawlAsync Is Started");
        // Start the crawling process
        try
        {
            CrawlResults = await siteCrawler.CrawlAsync(MaxPagesCrawled, StartPath).ConfigureAwait(true);
        }
        finally
        {
            // Notify clients that crawling has finished
            IsCrawling = false;
            await hubContext.Clients.All.SendAsync("UrlFound", $"CrawlAsync Is Complete");

            // Pause for 3 secons to allow clients to see the final results
            await Task.Delay(3000);
        }

    }
}
