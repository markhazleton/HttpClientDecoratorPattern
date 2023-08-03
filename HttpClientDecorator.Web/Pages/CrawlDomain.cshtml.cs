using HttpClientCrawler.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace HttpClientDecorator.Web.Pages;

public class CrawlDomainModel : PageModel
{
    [BindProperty]
    public bool IsCrawling { get; set; }
    [BindProperty]
    public string Domain { get; set; }
    [BindProperty]
    public int MaxDepth { get; set; } = 3; // Default maximum depth for crawling

    public SiteCrawler crawler;
    public IHubContext<CrawlHub> hubContext { get; }
    public IHttpClientService HttpClientService { get; }

    public CrawlDomainModel(IHubContext<CrawlHub> hubContext, IHttpClientService service)
    {
        this.hubContext = hubContext;
        HttpClientService = service;
    }
    public async Task OnPostAsync()
    {
        // Notify clients that crawling has started
        await hubContext.Clients.All.SendAsync("updateCrawlingStatus", true);
        // Start the crawling process
        try
        {
            crawler = new SiteCrawler(Domain, hubContext, HttpClientService);
            await crawler.Crawl(MaxDepth).ConfigureAwait(true);
        }
        finally
        {
            // Notify clients that crawling has finished
            await hubContext.Clients.All.SendAsync("updateCrawlingStatus", false);
            await hubContext.Clients.All.SendAsync("UrlFound", $"Crawl Is Complete");

        }
    }
}
