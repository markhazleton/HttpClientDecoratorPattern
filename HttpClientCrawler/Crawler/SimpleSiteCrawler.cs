using HtmlAgilityPack;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Web;

namespace HttpClientCrawler.Crawler;

public class SimpleSiteCrawler : ISiteCrawler
{
    private static readonly HashSet<string> crawledURLs = [];
    private static readonly ConcurrentQueue<CrawlResult> crawlQueue = new();
    private static readonly object lockObj = new();
    private static readonly ConcurrentDictionary<string, CrawlResult> resultsDict = new();
    private readonly IHttpClientFactory httpClientFactory;

    public SimpleSiteCrawler(IHttpClientFactory factory)
    {
        httpClientFactory = factory;
    }

    private bool AddCrawlResult(CrawlResult? result)
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
            SavePageFireAndForget(result);

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

    public async Task<ICollection<CrawlResult>> CrawlAsync(int MaxCrawlDepth, string link, CancellationToken ct = default)
    {
        var firstCrawl = new CrawlResult(link, link, 1, 1);
        var firstCrawlResult = await CrawlPage(firstCrawl, ct);
        if (firstCrawlResult is not null)
        {
            AddCrawlResult(firstCrawlResult);

            Console.WriteLine($"--First CrawlAsync Completed--{firstCrawlResult.RequestPath} -- found {firstCrawlResult.CrawlLinks.Count} links");

            int id = 1;
            id = await CrawlFoundLinks(firstCrawlResult, id, ct);
            Console.WriteLine($"****Depth 1 CrawlAsync Completed****");

            id = await CrawlAllFoundLinks(id, ct);
            Console.WriteLine($"****Depth 2 CrawlAsync Completed****");

            id = await ProcessCrawlQueue(id, ct);
            Console.WriteLine($"****Depth 3 CrawlAsync Completed****");

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

    public async Task SavePageAsync(CrawlResult result)
    {
        if (result == null || string.IsNullOrWhiteSpace(result.ResponseResults))
        {
            Console.WriteLine("No content to save or result is null.");
            return;
        }

        // Ensure the directory exists
        string directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pages");
        Directory.CreateDirectory(directoryPath);

        // Create a safe filename from the URL
        string safeFileName = GetSafeFileName(result.RequestPath);
        string filePath = Path.Combine(directoryPath, safeFileName);

        // Resolve relative links
        string updatedHtmlContent = ResolveRelativeLinks(result.ResponseResults, result.RequestPath);

        // Write the content to the file
        await File.WriteAllTextAsync(filePath, updatedHtmlContent);
        Console.WriteLine($"Saved {result.RequestPath} to {filePath}");
    }

    private string GetSafeFileName(string url)
    {
        try
        {
            var uri = new Uri(url);
            string path = uri.AbsolutePath; // Gets the absolute path component of the URL

            // Removing query strings and fragments from the path
            if (path.Contains("?"))
            {
                path = path.Substring(0, path.IndexOf("?"));
            }
            else if (path.Contains("#"))
            {
                path = path.Substring(0, path.IndexOf("#"));
            }

            // Remove the leading slash
            if (path.StartsWith("/"))
            {
                path = path.TrimStart('/');
            }

            // Use 'index.html' when path is empty or '/'
            if (string.IsNullOrEmpty(path) || path == "/")
            {
                return "index.html";
            }

            // Use URL encoding to ensure safe file names, then replace % with an underscore to make it more readable
            string encodedPath = HttpUtility.UrlEncode(path).Replace("%", "_");

            // Limit the length to avoid issues with file system limitations
            encodedPath = encodedPath.Length <= 150 ? encodedPath : encodedPath.Substring(0, 150);

            // Ensure the filename ends with only one ".html"
            if (!encodedPath.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
            {
                encodedPath += ".html";
            }

            return encodedPath;
        }
        catch
        {
            // In case of an exception (e.g., invalid URL), return a generic safe name
            return "default_safe_name.html";
        }
    }

    private string ResolveRelativeLinks(string htmlContent, string baseUrl)
    {
        var baseUri = new Uri(baseUrl);
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(htmlContent);

        // Include link elements with href attributes in the XPath query
        var nodes = htmlDoc.DocumentNode.SelectNodes("//a[@href]|//img[@src]|//link[@href]");
        if (nodes != null)
        {
            foreach (var node in nodes)
            {
                // Determine the attribute name (href or src)
                string attributeName = node.Name == "link" || node.Name == "a" ? "href" : "src";
                string originalValue = node.Attributes[attributeName]?.Value;
                if (!string.IsNullOrEmpty(originalValue) && Uri.TryCreate(originalValue, UriKind.Relative, out Uri relativeUri))
                {
                    var absoluteUri = new Uri(baseUri, relativeUri);
                    node.Attributes[attributeName].Value = absoluteUri.AbsoluteUri;
                }
            }
        }

        return htmlDoc.DocumentNode.OuterHtml;
    }
    public void SavePageFireAndForget(CrawlResult result)
    {
        Task.Run(async () =>
        {
            try
            {
                await SavePageAsync(result);
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                Console.WriteLine($"An error occurred while saving the page: {ex.Message}");
            }
        }).ConfigureAwait(false); // ConfigureAwait(false) to run the continuation on a ThreadPool thread
    }
}