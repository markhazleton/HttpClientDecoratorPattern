using HttpClientCrawler.Crawler;
using Microsoft.Extensions.DependencyInjection;

string domain = "https://wichitainformationcenter.com";
var serviceProvider = new ServiceCollection().AddHttpClient().BuildServiceProvider();

var crawler = new SimpleSiteCrawler(serviceProvider.GetService<IHttpClientFactory>());
var CrawlResults = await crawler.CrawlAsync(5, domain);

SiteCrawlerHelpers.WriteToCsv(CrawlResults, $"{SiteCrawlerHelpers.GetDomainName(domain)}_crawled_links.csv");
