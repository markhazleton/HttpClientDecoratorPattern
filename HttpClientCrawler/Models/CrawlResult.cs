using HtmlAgilityPack;
using HttpClientDecorator.Models;

namespace HttpClientCrawler.Models;

public class CrawlResult : HttpClientSendRequest<string>
{
    public List<string> ResponseLinks { get; } = new List<string>();

    public CrawlResult() : base()
    {
    }

    public CrawlResult(HttpClientSendRequest<string> statusCall) : base(statusCall)
    {
    }

    public List<string> CrawlLinks
    {
        get
        {
            ResponseLinks.Clear();
            if (ResponseHtmlDocument != null)
            {
                foreach (var link in ResponseHtmlDocument.DocumentNode
                    .Descendants("a")
                    .Select(a => RemoveQueryAndOnPageLinks(a.GetAttributeValue("href", null)))
                    .Where(link => !string.IsNullOrWhiteSpace(link))
                    )
                {
                    if (ResponseLinks.Contains(link))
                    {
                        continue;
                    }
                    if (IsValidLink(link))
                    {
                        if (IsSameDomain(link))
                        {
                            ResponseLinks.Add(link);
                        }
                    }
                }
            }
            return ResponseLinks;
        }
    }

    private string RemoveQueryAndOnPageLinks(string? link)
    {
         if (string.IsNullOrWhiteSpace(link)) return string.Empty;

        // Remove query parameters (if any)
        int queryIndex = link.IndexOf('?');
        if (queryIndex >= 0)
        {
            link = link[..queryIndex];
        }

        // Remove on-page links (if any)
        int hashIndex = link.IndexOf('#');
        if (hashIndex >= 0)
        {
            link = link[..hashIndex];
        }

        // Convert relative links to absolute links using the base domain
        if (!link.StartsWith("//") && Uri.TryCreate(link, UriKind.Relative, out var relativeUri))
        {
            Uri baseUri = new(this.RequestPath);
            Uri absoluteUri = new(baseUri, relativeUri);
            link = absoluteUri.ToString();
        }

        // Remove trailing '/' (if any)
        link = link.TrimEnd('/');

        return link.ToLower();
    }


    private bool IsSameDomain(string url)
    {
        if (!Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri))
        {
            return false; // Invalid URL, not the same _domain
        }

        if (url.StartsWith("//"))
        {
            // Replace "//" with "https://" and check if the modified URL is the same domain
            url = "https:" + url;
            return IsSameFullDomain(new Uri(url));
        }

        // If the URI is relative, treat it as the same domain
        if (!uri.IsAbsoluteUri)
        {
            return true;
        }

        // Handle partial scheme links
        if (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
        {
            return IsSameFullDomain(uri);
        }
        else if (url.StartsWith("//"))
        {
            // Replace "//" with "https://" and check if the modified URL is the same domain
            url = "https:" + url;
            return IsSameFullDomain(new Uri(url));
        }

        // For other schemes (e.g., "ftp", "mailto", etc.), treat them as different domains
        return false;
    }

    private bool IsSameFullDomain(Uri uri)
    {
        string host = new Uri(this.RequestPath).Host;
        string targetHost = uri.Host;

        // Check if the target host matches the _domain host and the URL has a valid path
        return string.Equals(host, targetHost, StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(uri.AbsolutePath)
            && uri.AbsolutePath != "/";
    }

    private bool IsValidLink(string link)
    {
        // Check if the link either has no extension or has .html or .htm extension
        string extension = Path.GetExtension(link);
        if (string.IsNullOrEmpty(extension) || extension.Equals(".html", StringComparison.OrdinalIgnoreCase) || extension.Equals(".htm", StringComparison.OrdinalIgnoreCase))
        {
            // Exclude image, XML, and video links
            return !link.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                && !link.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                && !link.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
                && !link.EndsWith(".gif", StringComparison.OrdinalIgnoreCase)
                && !link.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)
                && !link.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase)
                && !link.EndsWith(".avi", StringComparison.OrdinalIgnoreCase)
                && !link.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase)
                && !link.EndsWith(".mov", StringComparison.OrdinalIgnoreCase)
                && !link.Contains("/cdn-cgi/");
        }
        return false;
    }

    public HtmlDocument? ResponseHtmlDocument
    {
        get
        {
            try
            {
                HtmlDocument htmlDoc = new();
                htmlDoc.LoadHtml(ResponseResults);
                return htmlDoc;
            }
            catch
            {
                return null;
            }
        }
    }

}
