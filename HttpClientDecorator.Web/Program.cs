global using HttpClientDecorator;
global using HttpClientDecorator.Interfaces;
global using HttpClientDecorator.Models;
global using Microsoft.AspNetCore.Mvc.RazorPages;
global using Microsoft.Extensions.Caching.Memory;
global using System.Text.Json;
global using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();

// Add the HttpGetCall and Telemetry Decorator for IHttpClientRequestService interface
// Add Http Client Factory
builder.Services.AddHttpClient("HttpClientDecorator", client =>
{
    client.Timeout = TimeSpan.FromMilliseconds(3000);

    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("User-Agent", "HttpClientDecorator");
    client.DefaultRequestHeaders.Add("X-Request-ID", Guid.NewGuid().ToString());
    client.DefaultRequestHeaders.Add("X-Request-Source", "HttpClientDecorator");
});

builder.Services.AddSingleton(serviceProvider =>
{
    // Get the configuration options
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var retryOptions = configuration.GetSection("HttpClientSendPollyOptions").Get<HttpClientSendPollyOptions>();

    IHttpClientRequestService baseService = new HttpClientSendService(
        serviceProvider.GetRequiredService<ILogger<HttpClientSendService>>(),
        serviceProvider.GetRequiredService<IHttpClientFactory>());
    IHttpClientRequestService pollyService = new HttpClientSendServicePolly(
        serviceProvider.GetRequiredService<ILogger<HttpClientSendServicePolly>>(),
        baseService,
        retryOptions);
    IHttpClientRequestService telemetryService = new HttpClientSendServiceTelemetry(
        serviceProvider.GetRequiredService<ILogger<HttpClientSendServiceTelemetry>>(),
        pollyService);
    IHttpClientRequestService cacheService = new HttpClientSendServiceCache(
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
app.UseAuthorization();
app.UseCookiePolicy();
app.UseSession();
app.MapRazorPages();
app.Run();
