using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;

namespace HttpClientCrawler.Crawler;

public class SimpleSiteCrawler : ISiteCrawler
{
    private static readonly HashSet<string> crawledURLs = new();
    private static readonly ConcurrentQueue<CrawlResult> crawlQueue = new();
    private static readonly object lockObj = new();
    private static readonly ConcurrentDictionary<string, CrawlResult> resultsDict = new();
    private readonly IHttpClientFactory httpClientFactory;

    public SimpleSiteCrawler(IHttpClientFactory factory)
    {
        httpClientFactory = factory;
    }

    private static bool AddCrawlResult(CrawlResult? result)
    {
        if (result is null)
        {
            return false;
        }
        lock (lockObj)
        {
            if (resultsDict.ContainsKey(result.RequestPath))
            {
                return false;
            }
            resultsDict[result.RequestPath] = result;

            foreach (var foundUrl in result.CrawlLinks.ToArray())
            {
                if (crawledURLs.Contains(foundUrl))
                {
                    continue;
                }
                if (resultsDict.ContainsKey(foundUrl))
                {
                    continue;
                }

                if (crawlQueue.Any(w => w.RequestPath == foundUrl))
                {
                    continue;
                }
                var newCrawl = new CrawlResult(foundUrl, result.RequestPath, result.Depth + 1, crawledURLs.Count + 1);
                crawlQueue.Enqueue(newCrawl);
            }
            Console.WriteLine($"ID:{result.Id} CRAWLED:{resultsDict.Count:D5} QUEUE:{crawlQueue.Count:D5}  DEPTH:{result.Depth} TIME:{result.ElapsedMilliseconds:0,000} +++ Added Result: {result.RequestPath}");
            return true;
        }
    }
    private async Task<CrawlResult?> CrawlPage(CrawlResult? crawlResult, CancellationToken ct = default)
    {
        if (crawlResult is null)
        {
            return null;
        }
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        try
        {
            crawledURLs.Add(crawlResult.RequestPath);
            var response = await httpClientFactory.CreateClient("SimpleSiteCrawler").GetAsync(crawlResult.RequestPath, ct);
            crawlResult.StatusCode = response.StatusCode;

            if (response.IsSuccessStatusCode)
            {
                crawlResult.ResponseResults = await response.Content.ReadAsStringAsync(ct);
            }
        }
        catch (HttpRequestException ex)
        {
            // Handle HTTP errors
            if (Enum.TryParse(ex.StatusCode.ToString(), out HttpStatusCode statusCode))
            {
                crawlResult.StatusCode = statusCode;
            }
            else
            {
                crawlResult.StatusCode = HttpStatusCode.InternalServerError;
            }
            crawlResult.Errors.Add(ex.Message);
            crawlResult.Errors.Add("Error accessing page: " + crawlResult.RequestPath);
            Console.WriteLine(ex.Message);
        }
        catch (Exception ex)
        {
            crawlResult.StatusCode = HttpStatusCode.InternalServerError;
            crawlResult.Errors.Add(ex.Message);
            crawlResult.Errors.Add("Error accessing page: " + crawlResult.RequestPath);
            Console.WriteLine(ex.Message);
        }
        finally
        {
            stopwatch.Stop();
            crawlResult.ElapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            crawlResult.CompletionDate = DateTime.Now;
        }
        return crawlResult;
    }

    public async Task<ICollection<CrawlResult>> Crawl(int MaxCrawlDepth, string link, CancellationToken ct = default)
    {
        var firstCrawl = new CrawlResult(link, link, 1, 1);
        var firstCrawlResult = await CrawlPage(firstCrawl, ct);
        if (firstCrawlResult is not null)
        {
            AddCrawlResult(firstCrawlResult);

            Console.WriteLine($"--First Crawl Completed--{firstCrawlResult.RequestPath} -- found {firstCrawlResult.CrawlLinks.Count} links");

            int id = 1;
            id = await CrawlFoundLinks(firstCrawlResult, id, ct);
            Console.WriteLine($"****Depth 1 Crawl Completed****");

            id = await CrawlAllFoundLinks(id, ct);
            Console.WriteLine($"****Depth 2 Crawl Completed****");

            id = await ProcessCrawlQueue(id, ct);
            Console.WriteLine($"****Depth 3 Crawl Completed****");

        }
        return resultsDict.Values;
    }

    private async Task<int> ProcessCrawlQueue(int id, CancellationToken ct)
    {
        while (true)
        {
            if (crawlQueue.TryDequeue(out CrawlResult? crawlNext))
            {
                if (crawlNext is null)
                {
                    break;
                }

                if (crawledURLs.Contains(crawlNext.RequestPath))
                {
                    continue;
                }
                crawlNext.Id = id++;
                var queueCrawlResult = await CrawlPage(crawlNext, ct);
                if (queueCrawlResult is not null)
                {
                    AddCrawlResult(queueCrawlResult);
                }
            }
            else
            {
                break;
            }
        }
        return id;
    }

    private async Task<int> CrawlAllFoundLinks(int id, CancellationToken ct)
    {
        foreach (var item in resultsDict.Values)
        {
            foreach (var childLink in item.CrawlLinks.ToArray())
            {
                if (crawledURLs.Contains(childLink))
                {
                    continue;
                }
                var childCrawl = new CrawlResult(childLink, item.RequestPath, 3, id++);
                var childCrawlResult = await CrawlPage(childCrawl, ct);
                if (childCrawlResult is not null)
                {
                    AddCrawlResult(childCrawlResult);
                }
            }
        }
        return id;
    }

    private async Task<int> CrawlFoundLinks(CrawlResult? crawlResult, int id, CancellationToken ct)
    {
        if (crawlResult is null) { return id; }
        foreach (var childLink in crawlResult.CrawlLinks.ToArray())
        {
            if (crawledURLs.Contains(childLink)) { continue; }
            var childCrawl = new CrawlResult(childLink, crawlResult.RequestPath, 2, id++);
            var childCrawlResult = await CrawlPage(childCrawl, ct);
            if (childCrawlResult is not null)
            {
                AddCrawlResult(childCrawlResult);
            }
        }
        return id;
    }
}