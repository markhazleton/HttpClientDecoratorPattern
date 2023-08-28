using HttpClientCrawler.Crawler;
using Microsoft.Extensions.DependencyInjection;

string domain = "https://frogsfolly.com";
var serviceProvider = new ServiceCollection().AddHttpClient().BuildServiceProvider();

var crawler = new SimpleSiteCrawler.Crawler.SimpleSiteCrawler(serviceProvider.GetService<IHttpClientFactory>());
var CrawlResults = await crawler.Crawl(domain);

SiteCrawlerHelpers.WriteToCsv(CrawlResults, $"{SiteCrawlerHelpers.GetDomainName(domain)}_crawled_links.csv");
