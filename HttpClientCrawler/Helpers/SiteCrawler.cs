using HttpClientCrawler.Models;
using HttpClientDecorator.Interfaces;
using HttpClientDecorator.Models;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Net;

namespace HttpClientCrawler.Helpers;

public partial class SiteCrawler : ISiteCrawler
{
    public const int maxCrawlCount = 25;
    private readonly IHubContext<CrawlHub> _hubContext;
    private readonly IHttpClientService _service;

    public SiteCrawler(IHubContext<CrawlHub> hubContext, IHttpClientService httpClientService)
    {
        _hubContext = hubContext;
        _service = httpClientService;
    }

    private async Task<CrawlResult> CrawlPage(string url, int depth, CancellationToken ct = default)
    {
        CrawlResult crawlRequest = new()
        {
            CacheDurationMinutes = 0,
            RequestPath = url,
            Iteration = depth
        };

        try
        {
            crawlRequest = new CrawlResult(await _service.HttpClientSendAsync((HttpClientSendRequest<string>)crawlRequest, ct).ConfigureAwait(false));
        }
        catch (HttpRequestException ex)
        {
            crawlRequest.StatusCode = HttpStatusCode.ServiceUnavailable;
            Console.WriteLine("Error accessing page: " + url);
            Console.WriteLine(ex.Message);
        }
        catch (Exception ex)
        {
            crawlRequest.StatusCode = HttpStatusCode.InternalServerError;
            Console.WriteLine("Error accessing page: " + url);
            Console.WriteLine(ex.Message);
        }
        finally
        {
        }
        return crawlRequest;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="maxNumberOfResults"></param>
    /// <param name="StartPath"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<ICollection<CrawlResult>> Crawl(int maxNumberOfResults, string StartPath, CancellationToken ct = default)
    {
        Queue<string> _linksToCrawl = new();
        ConcurrentDictionary<string, CrawlResult> _crawlResults = new();

        _linksToCrawl.Enqueue(StartPath.ToLower());
        try
        {
            while (_linksToCrawl.Count > 0 && _crawlResults.Count<=maxNumberOfResults)
            {
                await Task.Delay(100, ct);

                string link = _linksToCrawl.Dequeue();
                var crawlResult = await CrawlPage(link, depth: _crawlResults.Count, ct: ct);

                _crawlResults.TryAdd(link, crawlResult);

                if (crawlResult.CrawlLinks.Count > 0 )
                {
                    foreach (var crawlLink in crawlResult.CrawlLinks)
                    {
                        if (!_crawlResults.ContainsKey(crawlLink))
                        {
                            if (!_linksToCrawl.Contains(crawlLink))
                            {
                                _linksToCrawl.Enqueue(crawlLink);
                            }
                        }
                    }
                }
                await _hubContext.Clients.All.SendAsync("UrlFound", $"Links to parse:{_linksToCrawl.Count} Crawled:{_crawlResults.Count} ", cancellationToken: ct);
            }
        }
        finally
        {
            await _hubContext.Clients.All.SendAsync("UrlFound", $"Crawl Complete:Crawled:{_crawlResults.Count} links", cancellationToken: ct);
        }
        return _crawlResults.Values;
    }
}