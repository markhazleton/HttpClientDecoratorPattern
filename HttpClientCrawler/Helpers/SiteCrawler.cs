using HttpClientCrawler.Models;
using HttpClientDecorator.Interfaces;
using HttpClientDecorator.Models;
using Microsoft.AspNetCore.SignalR;
using System.Net;

namespace HttpClientCrawler.Helpers;

public partial class SiteCrawler : ISiteCrawler
{
    public const int maxCrawlCount = 500;
    private readonly string _domain;
    private readonly IHubContext<CrawlHub> _hubContext;
    private readonly Queue<string> _linksToParse;
    private readonly IHttpClientService _service;
    private readonly HashSet<string> _visitedLinks;
    public readonly List<CrawlResult> CrawlResults;

    public SiteCrawler(string domain, IHubContext<CrawlHub> hubContext, IHttpClientService httpClientService)
    {
        _domain = domain;
        _visitedLinks = new HashSet<string>();
        _linksToParse = new Queue<string>();
        _hubContext = hubContext;
        _service = httpClientService;
        CrawlResults = new List<CrawlResult>();
    }

    private async Task CrawlPage(string url, int depth, CancellationToken ct = default)
    {
        if (_visitedLinks.Count >= maxCrawlCount || depth <= 0)
        {
            return;
        }

        CrawlResult crawlResult = new();
        if (!_visitedLinks.Contains(url))
        {
            _visitedLinks.Add(url);

            CrawlResult crawlRequest = new()
            {
                CacheDurationMinutes = 0,
                RequestPath = url,
                Iteration = _visitedLinks.Count
            };
            try
            {
                crawlResult = new CrawlResult(await _service.HttpClientSendAsync((HttpClientSendRequest<string>)crawlRequest, ct).ConfigureAwait(false));

                if (crawlResult.ErrorList.Count == 0)
                {
                    foreach (string link in ProcessLinks(crawlResult.CrawlLinks))
                    {
                        if (!_visitedLinks.Contains(link))
                        {
                            crawlResult.FoundLinks.Add(link);

                            // Use a priority queue (e.g., MinHeap) instead of a regular queue
                            _linksToParse.Enqueue(link);

                            // Notify the caller about the number of URLs found
                            await _hubContext.Clients.All.SendAsync("UrlFound", $"Links to parse:{_linksToParse.Count} Crawled:{_visitedLinks.Count} Depth:{depth}", cancellationToken: ct);

                            // Pause for 1/2 second to avoid overloading the server
                            await Task.Delay(500, ct).ConfigureAwait(false);

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
                CrawlResults.Add(crawlResult);
            }
        }
    }

    private List<string> ProcessLinks(List<string> links)
    {
        List<string> result = new();
        foreach (string link in links)
        {
            if (!_visitedLinks.Contains(link)
                && !_linksToParse.Contains(link)
                && !link.StartsWith("#"))
            {
                result.Add(link.ToLower());
            }
        }
        return result;
    }

    public async Task Crawl(int maxCrawlDepth, CancellationToken ct = default)
    {
        _linksToParse.Enqueue(_domain); // Enqueue the starting _domain
        try
        {
            while (_linksToParse.Count > 0)
            {
                string link = _linksToParse.Dequeue();
                await CrawlPage(link, depth: maxCrawlDepth, ct: ct);
            }
        }
        finally
        {
            await _hubContext.Clients.All.SendAsync("UrlFound", $"Links to parse:{_linksToParse.Count} Crawled:{CrawlResults.Count}", cancellationToken: ct);

        }

    }

    public async Task ExportToCSV(string filePath)
    {
        using (var writer = new StreamWriter(filePath))
        {
            await writer.WriteLineAsync("URL,Status Code,Elapsed Time,Crawl Date,Found Links");

            foreach (var result in CrawlResults)
            {
                var line = $"{result.RequestPath},{result.StatusCode},{result.ElapsedMilliseconds},{result.CompletionDate},{result.FoundLinks.Count}";
                await writer.WriteLineAsync(line);
            }
        }
        Console.WriteLine($"Crawl results with {CrawlResults.Count} pages exported to: {filePath}");
    }
}