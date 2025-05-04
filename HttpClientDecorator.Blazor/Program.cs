using HttpClientDecorator.Blazor.Components;
using Microsoft.Extensions.Caching.Memory;
using WebSpark.HttpClientUtility.RequestResult;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add additional services
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();
builder.Services.AddSignalR();

builder.Services.AddLogging(builder =>
{
    // Set the minimum logging level here
    builder.SetMinimumLevel(LogLevel.Warning);
    builder.AddConsole();
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

// HTTP Send Service with Decorator Pattern
builder.Services.AddSingleton(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var retryOptions = configuration.GetSection("HttpRequestResultPollyOptions").Get<HttpRequestResultPollyOptions>();

    IHttpRequestResultService baseService = new HttpRequestResultService(
        serviceProvider.GetRequiredService<ILogger<HttpRequestResultService>>(),
        configuration,
        serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient("HttpClientDecorator"));

    IHttpRequestResultService pollyService = new HttpRequestResultServicePolly(
        serviceProvider.GetRequiredService<ILogger<HttpRequestResultServicePolly>>(),
        baseService,
        retryOptions);

    IHttpRequestResultService telemetryService = new HttpRequestResultServiceTelemetry(
        serviceProvider.GetRequiredService<ILogger<HttpRequestResultServiceTelemetry>>(),
        pollyService);

    IHttpRequestResultService cacheService = new HttpRequestResultServiceCache(
        telemetryService,
        serviceProvider.GetRequiredService<ILogger<HttpRequestResultServiceCache>>(),
        serviceProvider.GetRequiredService<IMemoryCache>());

    return cacheService;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCookiePolicy();
app.UseSession();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
