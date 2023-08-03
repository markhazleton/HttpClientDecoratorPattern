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
    private readonly string _domain;
    private readonly IHubContext<CrawlHub> _hubContext;
    private readonly Queue<string> _linksToCrawl;
    private readonly IHttpClientService _service;
    private readonly SemaphoreSlim _semaphoreSlim = new(5, 5);
    private readonly ConcurrentDictionary<string, CrawlResult> _crawlResults;

    public ICollection<CrawlResult> CrawlResults 
    { 
        get { return _crawlResults.Values; }
    }

    public SiteCrawler(string domain, IHubContext<CrawlHub> hubContext, IHttpClientService httpClientService)
    {
        _domain = domain;
        _linksToCrawl = new Queue<string>();
        _hubContext = hubContext;
        _service = httpClientService;
        _crawlResults = new ConcurrentDictionary<string, CrawlResult>();
    }

    private async Task CrawlPage(string url, int depth, CancellationToken ct = default)
    {
        if (_crawlResults.Count >= maxCrawlCount || depth <= 0)
        {
            return;
        }

        if (_crawlResults.ContainsKey(url))
        {
            return;
        }

        // Acquire the semaphore before making the request
        await _semaphoreSlim.WaitAsync(ct);

        try
        {
            CrawlResult crawlResult = new();
            if (!_crawlResults.ContainsKey(url))
            {
                CrawlResult crawlRequest = new()
                {
                    CacheDurationMinutes = 0,
                    RequestPath = url,
                    Iteration = _crawlResults.Count
                };
                try
                {
                    crawlResult = new CrawlResult(await _service.HttpClientSendAsync((HttpClientSendRequest<string>)crawlRequest, ct).ConfigureAwait(false));

                    if (crawlResult.ErrorList.Count == 0)
                    {
                        foreach (string link in ProcessLinks(crawlResult.CrawlLinks))
                        {
                            if (!_crawlResults.ContainsKey(link))
                            {
                                // Use a priority queue (e.g., MinHeap) instead of a regular queue
                                _linksToCrawl.Enqueue(link);

                                // Notify the caller about the number of URLs found
                                await _hubContext.Clients.All.SendAsync("UrlFound", $"Links to parse:{_linksToCrawl.Count} Crawled:{_crawlResults.Count} Depth:{depth}", cancellationToken: ct);

                                // Recursively crawl with decreased depth
                                await CrawlPage(link, depth - 1, ct);
                            }
                        }
                    }
                }
                catch (HttpRequestException ex)
                {
                    // Handle HTTP errors
                    crawlResult.StatusCode = HttpStatusCode.ServiceUnavailable;
                    Console.WriteLine("Error accessing page: " + url);
                    Console.WriteLine(ex.Message);
                }
                catch (Exception ex)
                {
                    crawlResult.StatusCode = HttpStatusCode.InternalServerError;
                    Console.WriteLine("Error accessing page: " + url);
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    _crawlResults.TryAdd(url, crawlResult);
                }
            }
        }
        finally
        {
            // Release the semaphore after the request is complete
            _semaphoreSlim.Release();
        }
    }

    private List<string> ProcessLinks(List<string> links)
    {
        List<string> result = new();
        foreach (string link in links)
        {
            if (!_crawlResults.ContainsKey(link))
            {
                result.Add(link.ToLower());
            }
        }
        return result;
    }

    public async Task Crawl(int maxCrawlDepth, CancellationToken ct = default)
    {
        _linksToCrawl.Enqueue(_domain); // Enqueue the starting _domain
        try
        {
            while (_linksToCrawl.Count > 0)
            {
                string link = _linksToCrawl.Dequeue();
                await CrawlPage(link, depth: maxCrawlDepth, ct: ct);
            }
        }
        finally
        {
            await _hubContext.Clients.All.SendAsync("UrlFound", $"Crawl Complete:Crawled:{_crawlResults.Count} links", cancellationToken: ct);
        }

    }
}