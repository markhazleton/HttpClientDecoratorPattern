global using HttpClientDecorator;
global using HttpClientDecorator.Interfaces;
global using HttpClientDecorator.Models;
global using Microsoft.AspNetCore.Mvc.RazorPages;
global using Microsoft.AspNetCore.SignalR;
global using Microsoft.Extensions.Caching.Memory;
global using System.Text.Json;
global using System.Text.Json.Serialization;
using HttpClientCrawler.Crawler;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();
builder.Services.AddSignalR();

builder.Services.AddLogging(builder =>
{
    // Set the minimum logging level here
    builder.SetMinimumLevel(LogLevel.Warning); // Change LogLevel as needed
    builder.AddConsole(); // Add Console logger
                          // Add other log providers if necessary
});

// Configure HttpClient with HttpClientHandler
builder.Services.AddHttpClient("HttpClientDecorator", client =>
{
    // Configure your client here if needed
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    AllowAutoRedirect = true,
    MaxAutomaticRedirections = 10
});



builder.Services.AddHttpClient("HttpClientDecorator", client =>
{
    client.Timeout = TimeSpan.FromMilliseconds(30000);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("User-Agent", "HttpClientDecorator");
    client.DefaultRequestHeaders.Add("X-Request-ID", Guid.NewGuid().ToString());
    client.DefaultRequestHeaders.Add("X-Request-Source", "HttpClientDecorator");
});

builder.Services.AddSingleton(serviceProvider =>
{
    IHttpClientService baseService = new HttpClientSendService(
        serviceProvider.GetRequiredService<ILogger<HttpClientSendService>>(),
        serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient("HttpClientDecorator"));

    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var retryOptions = configuration.GetSection("HttpClientSendPollyOptions").Get<HttpClientSendPollyOptions>();
    IHttpClientService pollyService = new HttpClientSendServicePolly(
        serviceProvider.GetRequiredService<ILogger<HttpClientSendServicePolly>>(),
        baseService,
        retryOptions);

    IHttpClientService telemetryService = new HttpClientSendServiceTelemetry(
        serviceProvider.GetRequiredService<ILogger<HttpClientSendServiceTelemetry>>(),
        pollyService);

    IHttpClientService cacheService = new HttpClientSendServiceCache(
        telemetryService,
        serviceProvider.GetRequiredService<ILogger<HttpClientSendServiceCache>>(),
        serviceProvider.GetRequiredService<IMemoryCache>());

    return cacheService;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCookiePolicy();
app.UseSession();
app.MapRazorPages();
app.MapHub<CrawlHub>("/crawlHub");
app.Run();

